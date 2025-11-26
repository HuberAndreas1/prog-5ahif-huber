using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.VisualBasic;

namespace AppServices.Importer;

/// <summary>
/// Interface for parsing timesheet files into objects
/// </summary>
public interface ITimesheetParser
{
    /// <summary>
    /// Parses CSV content into a list of TimeEntry objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <param name="existingEmployees">Existing employees in the database</param>
    /// <param name="existingProjects">Existing projects in the database</param>
    /// <returns>List of parsed TimeEntry objects</returns>
    /// <exception cref="TimesheetParseException">
    /// Thrown when file content is invalid.
    /// </exception>
    /// <remarks>
    /// Note that this method must link TimeEntry objects to existing
    /// Employee and Project entities from the database where possible.
    /// 
    /// If an Employee (based on Employee ID) exists in the database, the
    /// employee's name will be updated if it differs from the CSV content.
    /// If a Project from the CSV does not exist in the database, a new
    /// entity must be created.
    /// 
    /// Multiple time entries with the same project code MUST reference 
    /// the same Project object. You must ensure that duplicate Project
    /// entities are not created for the same project code.
    /// </remarks>
    IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects);
}

/// <summary>
/// Represents all possible validation errors for the time tracking import file format.
/// </summary>
public enum ImportFileError
{
    // Employee Identification Errors
    MissingEmployeeId,
    MissingEmployeeName,
    DuplicateEmployeeId,
    DuplicateEmployeeName,
    EmployeeIdTooLong,              // Max 5 characters
    EmployeeNameTooLong,            // Max 100 characters
    EmployeeIdNotNumeric,

    // Field Format Errors
    InvalidKeyValueFormat,          // Missing ": " separator
    LeadingWhitespace,
    TrailingWhitespace,
    UnknownKey,                     // e.g., DEPARTMENT, TIMESHEET (singular)
    EmptyValue,

    // Timesheet Section Errors
    MissingTimesheetSection,        // No TIMESHEETS sections found
    TimesheetSectionBeforeEmployeeData,
    EmptyTimesheetSection,          // TIMESHEETS section with no time entries

    // Date Errors
    InvalidDate,                    // Not YYYY-MM-DD or invalid date

    // Time Entry Field Errors
    IncorrectFieldCount,            // Not exactly 4 semicolon-delimited fields
    EmptyField,                     // One or more fields are empty

    // Time Format Errors
    InvalidTime,                    // Not HH:MM or invalid time
    EndTimeBeforeStartTime,         // Logical validation

    // Description Errors
    DescriptionNotQuoted,           // Missing opening or closing quotes
    DescriptionTooLong,             // Max 200 characters

    // Project Errors
    ProjectTooLong,                 // Max 20 characters
    ProjectQuoted,                  // Project should NOT be quoted
}

public class TimesheetParseException(ImportFileError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<ImportFileError, string> ErrorMessages = new()
    {
        { ImportFileError.MissingEmployeeId, "Employee ID is missing." },
        { ImportFileError.MissingEmployeeName, "Employee name is missing." },
        { ImportFileError.DuplicateEmployeeId, "Duplicate employee ID found." },
        { ImportFileError.DuplicateEmployeeName, "Duplicate employee name found." },
        { ImportFileError.EmployeeIdTooLong, "Employee ID exceeds maximum length of 5 characters." },
        { ImportFileError.EmployeeNameTooLong, "Employee name exceeds maximum length of 100 characters." },
        { ImportFileError.EmployeeIdNotNumeric, "Employee ID must be numeric." },
        { ImportFileError.InvalidKeyValueFormat, "Invalid key-value format; missing ': ' separator." },
        { ImportFileError.LeadingWhitespace, "Leading whitespace detected in field." },
        { ImportFileError.TrailingWhitespace, "Trailing whitespace detected in field." },
        { ImportFileError.UnknownKey, "Unknown key found in the file." },
        { ImportFileError.EmptyValue, "Field value cannot be empty." },
        { ImportFileError.MissingTimesheetSection, "No TIMESHEETS section found in the file." },
        { ImportFileError.TimesheetSectionBeforeEmployeeData, "TIMESHEETS section appears before employee data." },
        { ImportFileError.EmptyTimesheetSection, "TIMESHEETS section is empty." },
        { ImportFileError.InvalidDate, "Invalid date format; expected YYYY-MM-DD." },
        { ImportFileError.IncorrectFieldCount, "Incorrect number of fields in time entry; expected 4 fields." },
        { ImportFileError.EmptyField, "One or more fields in time entry are empty." },
        { ImportFileError.InvalidTime, "Invalid time format; expected HH:MM." },
        { ImportFileError.EndTimeBeforeStartTime, "End time is before start time." },
        { ImportFileError.DescriptionNotQuoted, "Description field must be enclosed in double quotes." },
        { ImportFileError.DescriptionTooLong, "Description exceeds maximum length of 200 characters." },
        { ImportFileError.ProjectTooLong, "Project code exceeds maximum length of 20 characters." },
        { ImportFileError.ProjectQuoted, "Project code must not be quoted." },
    };

    public ImportFileError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into TimeEntry objects
/// </summary>
public class TimesheetParser : ITimesheetParser
{
    /// <inheritdoc/>
    public IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects)
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries).ToArray();
        var index = 0;

        var (employeeId, employeeName) = ParseEmployeeSection(lines, ref index);

        if (index >= lines.Length)
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        var employee = FindOrCreateEmployee(employeeId, employeeName, existingEmployees);

        var projects = existingProjects.ToDictionary(p => p.ProjectCode, p => p);

        if (!lines[index].Contains(": "))
        {
            throw new TimesheetParseException(ImportFileError.InvalidKeyValueFormat);
        }

        var timeEntries = new List<TimeEntry>();

        while (index < lines.Length)
        {
            var timeSheetDate = ParseTimeSheetHeader(lines, ref index);
            var sectionTimeEntries = ParseTimeEntries(lines, ref index, timeSheetDate, employee, projects);

            timeEntries.AddRange(sectionTimeEntries);
        }

        return timeEntries;
    }

    private static List<TimeEntry> ParseTimeEntries(string[] lines, ref int index, DateOnly timeSheetDate, Employee employee, Dictionary<string, Project> projects)
    {
        if (index >= lines.Length)
        {
            throw new TimesheetParseException(ImportFileError.EmptyTimesheetSection);
        }

        var timeEntries = new List<TimeEntry>();

        while (index < lines.Length)
        {
            var record = lines[index];

            if (record.StartsWith("TIMESHEETS")) {
                break;
            }

            timeEntries.Add(ParseTimeEntry(record, timeSheetDate, employee, projects));

            index++;
        }

        return timeEntries;
    }

    private static TimeEntry ParseTimeEntry(string record, DateOnly timeSheetDate, Employee employee, Dictionary<string, Project> projects)
    {
        var splittedRecord = record.Split(";", StringSplitOptions.None);

        if (splittedRecord.Length != 4)
        {
            throw new TimesheetParseException(ImportFileError.IncorrectFieldCount);
        }

        if (splittedRecord.Any(string.IsNullOrWhiteSpace)) {
            throw new TimesheetParseException(ImportFileError.EmptyField);
        }

        if (!TimeOnly.TryParseExact(splittedRecord[0], "HH:mm", out var startTime)) {
            throw new TimesheetParseException(ImportFileError.InvalidTime);
        }

        if (!TimeOnly.TryParseExact(splittedRecord[1], "HH:mm", out var endTime)) {
            throw new TimesheetParseException(ImportFileError.InvalidTime);
        }

        if (startTime > endTime) {
            throw new TimesheetParseException(ImportFileError.EndTimeBeforeStartTime);
        }

        var description = ParseDescription(splittedRecord[2]);

        var project = ParseProject(splittedRecord[3], projects);

        return new TimeEntry
        {
            Date = timeSheetDate,
            StartTime = startTime,
            EndTime = endTime,
            Description = description,
            Employee = employee,
            Project = project,
            ProjectId = project.Id,
            EmployeeId = employee.Id
        };
    }

    private static Project ParseProject(string projectCode, Dictionary<string, Project> projects) {
        
        if (projectCode.StartsWith('"') || projectCode.EndsWith('"')) {
            throw new TimesheetParseException(ImportFileError.ProjectQuoted);
        }

        if (projectCode.Length > 20) {
            throw new TimesheetParseException(ImportFileError.ProjectTooLong);
        }

        if (!projects.TryGetValue(projectCode, out var projectObject)) {
            projectObject = new Project
            {
                ProjectCode = projectCode
            };
            projects.Add(projectCode, projectObject);
        }

        return projectObject;
    }
    private static string ParseDescription(string description) {
        if (!description.StartsWith('"') || !description.EndsWith('"')) {
            throw new TimesheetParseException(ImportFileError.DescriptionNotQuoted);
        }

        description = description[1..^1];

        if (description.Length > 200) {
            throw new TimesheetParseException(ImportFileError.DescriptionTooLong);
        }

        return description;
    }

    private static DateOnly ParseTimeSheetHeader(string[] lines, ref int index)
    {
        if (index >= lines.Length)
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        var line = lines[index];
        ValidateLineWhitespaces(line);

        if (!line.StartsWith("TIMESHEETS:"))
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        if (!DateOnly.TryParseExact(line["TIMESHEETS: ".Length..], "yyyy-MM-dd", out var date))
        {
            throw new TimesheetParseException(ImportFileError.InvalidDate);
        }

        index++;

        return date;
    }

    private static (string employeeId, string employeeName) ParseEmployeeSection(string[] lines, ref int index)
    {
        string? employeeId = null;
        string? employeeName = null;

        while (index < lines.Length)
        {
            var line = lines[index];

            if (line.StartsWith("TIMESHEETS:"))
            {
                break;
            }

            ValidateLineWhitespaces(line);

            if (!line.Contains(": "))
            {
                throw new TimesheetParseException(ImportFileError.InvalidKeyValueFormat);
            }

            var keyValueSplit = line.Split(": ");
            var key = keyValueSplit[0];
            var value = keyValueSplit[1];

            switch (key)
            {
                case "EMP-ID":

                    if (employeeId != null)
                    {
                        throw new TimesheetParseException(ImportFileError.DuplicateEmployeeId);
                    }

                    if (value.Length > 5)
                    {
                        throw new TimesheetParseException(ImportFileError.EmployeeIdTooLong);
                    }

                    if (!int.TryParse(value, out _))
                    {
                        throw new TimesheetParseException(ImportFileError.EmployeeIdNotNumeric);
                    }


                    employeeId = value;
                    break;
                case "EMP-NAME":
                    if (employeeName != null)
                    {
                        throw new TimesheetParseException(ImportFileError.DuplicateEmployeeName);
                    }

                    if (value.Length > 100)
                    {
                        throw new TimesheetParseException(ImportFileError.EmployeeIdTooLong);
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        throw new TimesheetParseException(ImportFileError.EmptyValue);
                    }

                    employeeName = value;
                    break;
                default:
                    throw new TimesheetParseException(ImportFileError.UnknownKey);
            }

            index++;
        }

        if (employeeId == null)
        {
            throw new TimesheetParseException(ImportFileError.MissingEmployeeId);
        }

        if (employeeName == null)
        {
            throw new TimesheetParseException(ImportFileError.MissingEmployeeName);
        }

        return (employeeId, employeeName);
    }
    private static void ValidateLineWhitespaces(string line)
    {

        if (line.Length > 0 && char.IsWhiteSpace(line[0]))
        {
            throw new TimesheetParseException(ImportFileError.LeadingWhitespace);
        }

        if (line.Length > 0 && char.IsWhiteSpace(line[^1]))
        {
            throw new TimesheetParseException(ImportFileError.TrailingWhitespace);
        }
    }

    private static Employee FindOrCreateEmployee(string employeeId, string employeeName, IEnumerable<Employee> existingEmployees)
    {
        var employee = existingEmployees.FirstOrDefault(e => e.EmplyeeId.Equals(employeeId));

        if (employee != null)
        {
            if (employee.EmployeeName != employeeName)
            {
                employee.EmployeeName = employeeName;
            }

            return employee;
        }

        employee = new Employee
        {
            EmplyeeId = employeeId,
            EmployeeName = employeeName
        };

        return employee;
    }
}
