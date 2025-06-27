using Film_website.Models;
using Film_website.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Film_website.Services
{
    public class MovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly string _uploadPath;

        public MovieService(IMovieRepository movieRepository, IWebHostEnvironment env)
        {
            _movieRepository = movieRepository;
            _uploadPath = Path.Combine(env.WebRootPath, "uploads/movies");
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
        {
            return await _movieRepository.GetAllAsync();
        }

        public async Task<Movie> GetMovieByIdAsync(int id)
        {
            return await _movieRepository.GetByIdAsync(id);
        }

        public async Task AddMovieAsync(Movie movie, IFormFile movieFile, IFormFile thumbnailFile)
        {
            if (movieFile == null || movieFile.Length == 0)
            {
                throw new ArgumentException("Movie file is required.");
            }

            // Kiểm tra định dạng file MP4
            if (!movieFile.FileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only MP4 files are allowed.");
            }

            try
            {
                var movieFileName = Guid.NewGuid().ToString() + Path.GetExtension(movieFile.FileName);
                var movieFilePath = Path.Combine(_uploadPath, movieFileName);
                using (var stream = new FileStream(movieFilePath, FileMode.Create))
                {
                    await movieFile.CopyToAsync(stream);
                }
                movie.FilePath = $"/uploads/movies/{movieFileName}";

                if (thumbnailFile != null && thumbnailFile.Length > 0)
                {
                    var thumbFileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                    var thumbFilePath = Path.Combine(_uploadPath, thumbFileName);
                    using (var stream = new FileStream(thumbFilePath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    movie.ThumbnailPath = $"/uploads/movies/{thumbFileName}";
                }

                await _movieRepository.AddAsync(movie);
            }
            catch (IOException ex)
            {
                throw new Exception("Error saving files to server.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding movie to database.", ex);
            }
        }

        public async Task UpdateMovieAsync(Movie movie, IFormFile movieFile, IFormFile thumbnailFile)
        {
            var existingMovie = await _movieRepository.GetByIdAsync(movie.Id);
            if (existingMovie == null)
                throw new Exception("Movie not found");

            if (movieFile != null)
            {
                // Delete old file if exists
                if (!string.IsNullOrEmpty(existingMovie.FilePath))
                {
                    var oldFilePath = Path.Combine(_uploadPath, Path.GetFileName(existingMovie.FilePath));
                    if (File.Exists(oldFilePath))
                        File.Delete(oldFilePath);
                }

                var movieFileName = Guid.NewGuid().ToString() + Path.GetExtension(movieFile.FileName);
                var movieFilePath = Path.Combine(_uploadPath, movieFileName);
                using (var stream = new FileStream(movieFilePath, FileMode.Create))
                {
                    await movieFile.CopyToAsync(stream);
                }
                movie.FilePath = $"/uploads/movies/{movieFileName}";
            }
            else
            {
                movie.FilePath = existingMovie.FilePath;
            }

            if (thumbnailFile != null)
            {
                if (!string.IsNullOrEmpty(existingMovie.ThumbnailPath))
                {
                    var oldThumbPath = Path.Combine(_uploadPath, Path.GetFileName(existingMovie.ThumbnailPath));
                    if (File.Exists(oldThumbPath)) // Correct variable name used here
                    {
                        File.Delete(oldThumbPath);
                    }
                }

                var thumbFileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                var thumbFilePath = Path.Combine(_uploadPath, thumbFileName);
                using (var stream = new FileStream(thumbFilePath, FileMode.Create))
                {
                    await thumbnailFile.CopyToAsync(stream);
                }
                movie.ThumbnailPath = $"/uploads/movies/{thumbFileName}";
            }
            else
            {
                movie.ThumbnailPath = existingMovie.ThumbnailPath;
            }

            movie.UpdatedAt = DateTime.UtcNow;
            await _movieRepository.UpdateAsync(movie);
        }

        public async Task DeleteMovieAsync(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie != null)
            {
                // Delete files
                if (!string.IsNullOrEmpty(movie.FilePath))
                {
                    var filePath = Path.Combine(_uploadPath, Path.GetFileName(movie.FilePath));
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                if (!string.IsNullOrEmpty(movie.ThumbnailPath))
                {
                    var thumbPath = Path.Combine(_uploadPath, Path.GetFileName(movie.ThumbnailPath));
                    if (File.Exists(thumbPath))
                    {
                        File.Delete(thumbPath);
                    }
                }

                await _movieRepository.DeleteAsync(id);
            }
        }
    }
}