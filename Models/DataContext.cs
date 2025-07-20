using Microsoft.EntityFrameworkCore;

namespace muhaberat_evrak_yonetim.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}