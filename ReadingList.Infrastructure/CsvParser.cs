using ReadingList.Domain;
using System.Globalization;
using System.Text;

namespace ReadingList.Infrastructure
{
    public static class CsvParser
    {
        private const string ExpectedHeader = "Id,Title,Author,Year,Pages,Genre,Finished,Rating";


        public static Result<bool> ValidateHeader(string headerLine)
        {

            if (headerLine is null)
            {
                return Result<bool>.Fail("Header line is null.");
            }

            var given = headerLine.Trim();

            var ok = string.Equals(given, ExpectedHeader, StringComparison.OrdinalIgnoreCase);

            return ok
                ? Result<bool>.Ok(true)
                : Result<bool>.Fail($"Invalid CSV header. Expected: '{ExpectedHeader}', but got: '{given}'.");
        }

        public static Result<Book> ParseLine(string line) 
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return Result<Book>.Fail("Empty csv line");
            }

            var cells = SplitCsvLine(line);

            if (cells.Length != 8)
            {
                return Result<Book>.Fail($"Invalid number of fields. Expected 8 but got {cells.Length}.");
            }

            if (!int.TryParse(cells[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || id <= 0)
            {
                return Result<Book>.Fail($"Invalid Id value.");
            }

            var title = cells[1].NormalizeSpaces().ToTitleCaseSafe();

            if (string.IsNullOrWhiteSpace(title))
                return Result<Book>.Fail("Title is required.");


            var author = cells[2].NormalizeSpaces().ToTitleCaseSafe();

            if (string.IsNullOrWhiteSpace(author))
                return Result<Book>.Fail("Author is required.");

            if (!int.TryParse(cells[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
                return Result<Book>.Fail("Invalid Year (must be an integer).");

            if (!int.TryParse(cells[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pages))
                return Result<Book>.Fail("Invalid Pages (must be an integer).");

            var genre = cells[5].NormalizeSpaces().ToTitleCaseSafe();

            if (string.IsNullOrWhiteSpace(genre))
                return Result<Book>.Fail("Genre is required.");

            var finishedStr = cells[6].Trim();
            bool finished = false;
            finishedStr.TryParseYesNo(out finished);

            double rating = 0.0;
            var ratingStr = cells[7].Trim();
            if (!string.IsNullOrEmpty(ratingStr))
            {
                if (!double.TryParse(ratingStr, NumberStyles.Float, CultureInfo.InvariantCulture, out rating))
                {
                    if (!double.TryParse(ratingStr, System.Globalization.NumberStyles.Float, CultureInfo.CurrentCulture, out rating))
                    {
                        return Result<Book>.Fail("Invalid Rating (use 4.5 or 4,5).");
                    }
                }
                if (rating < 0.0 || rating > 5.0)
                    return Result<Book>.Fail("Rating must be between 0.0 and 5.0.");
            }

            try
            {
                var book = new Book(
                    id: id,
                    title: title,
                    author: author,
                    year: year,
                    pages: pages,
                    genre: genre,
                    finished: finished,
                    rating: rating
                );

                return Result<Book>.Ok(book);
            }
            catch (Exception ex)
            {
                return Result<Book>.Fail($"Error creating Book object: {ex.Message}");
            }

        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            var stringBuilder = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        bool isNextQuote = (i + 1 < line.Length) && (line[i + 1] == '"');

                        if (isNextQuote)
                        {
                            stringBuilder.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }

                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(stringBuilder.ToString().Trim());
                        stringBuilder.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
            }

            result.Add(stringBuilder.ToString().Trim());
            return result.ToArray();


        }
    }
}
