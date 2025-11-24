using System.ComponentModel.DataAnnotations;


namespace ReadingList.Domain
{
    public class Book
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive numbers allowed")]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        [YearPublished]
        public int YearPublished { get; set; }
        [Range(1, 5000)]
        public int NumberOfPages { get; set; }
        public string Genre { get; set; }
        public bool IsFinished { get; set; }
        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0.")]  
        public double Rating { get; set; }


        public Book(
            int id,
            string title,
            string author,
            int year,
            int pages,
            string genre,
            bool finished = false,
            double rating = 0.0)
        {
            this.Id = id;
            this.Title = title;
            this.Author = author;
            this.YearPublished = year;
            this.NumberOfPages = pages;
            this.Genre = genre;
            this.IsFinished = finished;
            this.Rating = rating;

            Validate();
        }

        public void Validate()
        {
            var ctx = new ValidationContext(this);
            Validator.ValidateObject(this, ctx, validateAllProperties: true);
        }

        public void MarkAsFinished() => this.IsFinished = true;

        public void SetRating(double value)
        {
            if(value < 0.0 || value > 5.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Rating must be between 0.0 and 5.0");
            }
            this.Rating = Math.Round(value, 1);
        }

        public override string ToString()
        {
            return $" #{Id} | {Title} by {Author} ({YearPublished}), {NumberOfPages} pages, Genre: {Genre}, Finished: {IsFinished}, Rating: {Rating}/5.0 *";
        }

    }
}
