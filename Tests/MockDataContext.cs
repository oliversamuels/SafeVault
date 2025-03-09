namespace Tests;

using Microsoft.EntityFrameworkCore;
using Moq;
using SafeVault;
public class MockDataContext
{
    public static DataContext GetMockedDataContext()
    {
        var users = new List<User>
        {
            new User { Username = "testuser", Email = "test@example.com", PasswordHash = "hashedpassword" }
        }.AsQueryable();

        var mockSet = new Mock<DbSet<User>>();
        mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        var mockContext = new Mock<DataContext>();
        mockContext.Setup(c => c.Users).Returns(mockSet.Object);

        return mockContext.Object;
    }
}