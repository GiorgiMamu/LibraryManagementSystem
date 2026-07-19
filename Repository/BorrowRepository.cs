using System;
using System.Collections.Generic;
using System.Linq;
using Core.Enums;
using Core.Interfaces;
using Core.Models;

namespace Repository
{
    // File format: BorrowID | UserID | ISBN | ReturnDate | Status | FineLastCalculatedDate
    // ReturnDate and FineLastCalculatedDate are blank when not yet set
    public class BorrowRepository : IBorrowRepository
    {
        private readonly IFileManager _fileManager;
        private readonly ILogger _logger;
        private readonly string _path;

        public BorrowRepository(IFileManager fileManager, ILogger logger, string path = @"C:\Users\oto\Desktop\LibraryManagementSystem\Repository\Data\borrows.txt")
        {
            _fileManager = fileManager;
            _logger = logger;
            _path = path;
        }

        public List<BorrowRecord> GetAll()
        {
            var records = new List<BorrowRecord>();
            foreach (var line in _fileManager.ReadLines(_path))
            {
                var record = ParseLine(line);
                if (record != null) records.Add(record);
            }
            return records;
        }

        public BorrowRecord? GetById(string borrowId) =>
            GetAll().FirstOrDefault(r => r.BorrowId == borrowId);

        public List<BorrowRecord> GetByUserId(int userId) =>
            GetAll().Where(r => r.UserId == userId).ToList();

        public void Add(BorrowRecord record) => _fileManager.AppendLine(_path, ToLine(record));

        public void Update(BorrowRecord record)
        {
            var lines = GetAll()
                .Select(r => r.BorrowId == record.BorrowId ? ToLine(record) : ToLine(r))
                .ToList();
            _fileManager.WriteLines(_path, lines);
        }

        private BorrowRecord? ParseLine(string line)
        {
            try
            {
                var parts = line.Split('|').Select(p => p.Trim()).ToArray();
                if (parts.Length < 5) return null;

                var status = Enum.Parse<BorrowStatus>(parts[4], ignoreCase: true);
                DateTime? dueDate = string.IsNullOrWhiteSpace(parts[3]) ? null : DateTime.Parse(parts[3]);

                DateTime? fineLastCalculatedDate = (parts.Length >= 6 && !string.IsNullOrWhiteSpace(parts[5]))
                    ? DateTime.Parse(parts[5])
                    : null;

                return new BorrowRecord(parts[0], int.Parse(parts[1]), parts[2], dueDate, status, fineLastCalculatedDate);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Skipped malformed line in {_path}: '{line}'", ex);
                return null;
            }
        }

        private static string ToLine(BorrowRecord r)
        {
            string dueDateStr = r.DueDate.HasValue ? r.DueDate.Value.ToString("yyyy-MM-dd") : "";
            string fineDateStr = r.FineLastCalculatedDate.HasValue ? r.FineLastCalculatedDate.Value.ToString("yyyy-MM-dd") : "";
            return $"{r.BorrowId} | {r.UserId} | {r.Isbn} | {dueDateStr} | {r.Status} | {fineDateStr}";
        }
    }
}