using MongoDB.Driver;
using Moq;

namespace TradingPlatformAPI.Tests;

/// <summary>
/// Factory helpers that create pre-configured Moq mocks of MongoDB driver
/// interfaces, eliminating boilerplate in every test class.
/// </summary>
internal static class MongoTestHelper
{
    // ------------------------------------------------------------------ //
    // Collection factory
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Creates a <see cref="Mock{T}"/> for <see cref="IMongoCollection{T}"/>
    /// with <c>InsertOneAsync</c> and <c>ReplaceOneAsync</c> already stubbed to
    /// complete successfully.  Call <see cref="SetupFind{T}"/> afterwards to
    /// configure what <c>Find</c> returns.
    /// </summary>
    public static Mock<IMongoCollection<T>> CreateCollection<T>()
    {
        var mock = new Mock<IMongoCollection<T>>();

        mock.Setup(c => c.InsertOneAsync(
                It.IsAny<T>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<T>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ReplaceOneResult.Unacknowledged.Instance);

        return mock;
    }

    // ------------------------------------------------------------------ //
    // Find setup
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Configures <c>FindAsync</c> on <paramref name="mockCollection"/> so that
    /// successive calls consume result-sets from the supplied queue.
    /// Each <paramref name="resultSets"/> element is returned for one call;
    /// once the queue is empty, subsequent calls return an empty sequence.
    /// <para>
    /// In MongoDB.Driver 3.x <c>Find()</c> is an extension method that delegates
    /// to <c>FindAsync</c> internally, so mocking <c>FindAsync</c> is the
    /// correct interception point.
    /// </para>
    /// </summary>
    public static void SetupFind<T>(
        Mock<IMongoCollection<T>> mockCollection,
        params IEnumerable<T>[] resultSets)
    {
        var queue = new Queue<IEnumerable<T>>(resultSets);

        mockCollection
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<T>>(),
                It.IsAny<FindOptions<T, T>>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                var items = queue.Count > 0
                    ? queue.Dequeue().ToList()
                    : new List<T>();
                return Task.FromResult(BuildCursor(items));
            });
    }

    // ------------------------------------------------------------------ //
    // Database factory
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Adds a <c>GetCollection&lt;T&gt;(name, null)</c> setup to
    /// <paramref name="mockDb"/> (or creates a new one if <c>null</c>) and
    /// returns it.  Call this method once per collection type to build up a
    /// fully wired database mock.
    /// </summary>
    public static Mock<IMongoDatabase> RegisterCollection<T>(
        Mock<IMongoDatabase>? mockDb,
        string collectionName,
        Mock<IMongoCollection<T>> mockCollection)
    {
        mockDb ??= new Mock<IMongoDatabase>();
        mockDb.Setup(db => db.GetCollection<T>(collectionName, null))
              .Returns(mockCollection.Object);
        return mockDb;
    }

    // ------------------------------------------------------------------ //
    // Private helpers
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Wraps a list of items in a mock <see cref="IAsyncCursor{T}"/> that yields
    /// all items in a single batch and then signals end-of-cursor.
    /// </summary>
    private static IAsyncCursor<T> BuildCursor<T>(List<T> items)
    {
        var cursor = new Mock<IAsyncCursor<T>>();
        var exhausted = false;

        cursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync(() =>
              {
                  if (exhausted) return false;
                  exhausted = true;
                  return items.Count > 0;
              });

        cursor.Setup(c => c.Current).Returns(items);
        cursor.Setup(c => c.Dispose());

        return cursor.Object;
    }
}
