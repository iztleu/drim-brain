using Microsoft.EntityFrameworkCore;

namespace NotificationService.Database;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options);
