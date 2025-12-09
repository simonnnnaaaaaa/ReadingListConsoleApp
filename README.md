📚 ReadingList — Console Application for Managing a Personal Reading List

ReadingList is a clean and extensible C# console application that allows you to manage a collection of books, import them from CSV files, export them to JSON/CSV, and interact with them via powerful CLI commands.
The design emphasizes clean architecture, testability, validations, and zero “magic strings”.

✨ Features
📥 Import Books

Import one or multiple CSV files

Handles malformed lines gracefully

Detects duplicates

Validates fields using DataAnnotations (e.g., custom [YearPublished] attribute)

Detailed import summary

📤 Export Books

Export to JSON or CSV

Strategy pattern for extensible export formats

Overwrite protection when files already exist

📚 Manage Books

List all books

Filter finished books

Show top-rated books

Search by author

Mark a book as finished

Rate books (0–5 stars)

💾 Clean Repository Abstraction

In-memory repository for now

Easily replaceable with database or file-backed storage

🧪 Tests Included

Import logic tests

Duplicate detection

Malformed line detection

Validation support

🧼 No Magic Strings

Centralized message resources in a Messages.cs file

🏗️ Project Structure

ReadingList/
 ├── Domain/
 │    ├── Book.cs
 │    ├── YearPublishedAttribute.cs
 │    └── IRepository<T,TKey>.cs
 │
 ├── Infrastructure/
 │    ├── ImportService.cs
 │    ├── Exporting/
 │    │      ├── ExportService.cs
 │    │      ├── IExportStrategy.cs
 │    │      ├── JsonExportStrategy.cs
 │    │      └── CsvExportStrategy.cs
 │    └── InMemoryRepository.cs
 │
 ├── App/
 │    ├── Program.cs
 │    ├── Helpers/
 │    │      ├── CommandHandlers.cs
 │    │      ├── ConsolePrinter.cs
 │    └── Messages.cs
 │
 ├── Tests/
 │    └── ImportServiceTests.cs
 │
 ├── data/
 │    └── example CSV files
 │
 └── README.md

🚀 Getting Started
1. Clone the repo
git clone https://github.com/your-username/ReadingList.git
cd ReadingList

2. Build the project
dotnet build

3. Run the application
dotnet run --project ReadingList.App

🖥️ CLI Commands
import <path>

Imports a single CSV file.

Example:

import data/books.csv

import file1 file2 file3

Imports multiple CSV files in parallel.

list all

Lists all books in the repository.

filter finished

Shows only books marked as read.

top rated <n>

Shows the top n highest-rated books.

by author <text>

Searches for books whose author contains <text>.

mark finished <id>

Marks a book as finished.

rate <id> <value>

Assigns a rating between 0.0 and 5.0.

export json <path>

Exports the current list to a JSON file.

export csv <path>

Exports the current list to a CSV file.

📄 CSV Import Format

The first row must match the header:

Id,Title,Author,Year,Pages,Genre,Finished,Rating


Example:

1,"Clean Code",Robert C. Martin,2008,464,software,yes,5
2,"The Hobbit",J.R.R. Tolkien,1937,310,fantasy,no,4.5

🧩 Extensibility
Add new export formats

Implement:

public interface IExportStrategy
{
    string Format { get; }
    Task ExportAsync(IEnumerable<Book> books, string path, CancellationToken ct);
}


Then register it in:

new ExportService(new IExportStrategy[] { ... });

Replace repository with real storage

Implement IRepository<Book, int>.

🧪 Running Tests
dotnet test


The test suite includes:

Parallel import scenarios

Duplicate detection

Malformed row handling

Validation exception handling

🏅 Technologies Used

.NET 7+

C# 10/11 features

DataAnnotations (custom validation attributes)

Asynchronous file I/O

Strategy Pattern

Dependency Injection-friendly architecture

xUnit
