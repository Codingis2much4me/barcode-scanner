using System.Collections.Generic;
using StudentBarcodeApp.Models;
using System.Threading.Tasks;

namespace StudentBarcodeApp.Services
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync();
        Task<Student?> GetStudentByRollNumberAsync(string rollNumber);
        Task<List<Student>> GetAllStudentsAsync();
        Task AddStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(string rollNumber);
    }
}
