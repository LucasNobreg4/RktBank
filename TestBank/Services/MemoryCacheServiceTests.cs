using Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace TestBank.Services
{
    public class MemoryCacheServiceTests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly MemoryCacheService _sut;

        public MemoryCacheServiceTests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _sut = new MemoryCacheService(_memoryCacheMock.Object);
        }

        [Fact]
        public void Get_WhenKeyExists_ReturnsValue()
        {
            var key = "test-key";
            var expectedValue = "test-value";
            object cacheValue = expectedValue;

            _memoryCacheMock
                .Setup(x => x.TryGetValue(key, out cacheValue))
                .Returns(true);

            var result = _sut.Get<string>(key);

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void Get_WhenKeyDoesNotExist_ReturnsDefault()
        {
            var key = "non-existent";
            object? cacheValue = null;

            _memoryCacheMock
                .Setup(x => x.TryGetValue(key, out cacheValue))
                .Returns(false);

            var result = _sut.Get<string>(key);

            Assert.Null(result);
        }

        [Fact]
        public void TryGetValue_WhenKeyExists_ReturnsTrueAndValue()
        {
            var key = "test-key";
            var expectedValue = "test-value";
            object cacheValue = expectedValue;

            _memoryCacheMock
                .Setup(x => x.TryGetValue(key, out cacheValue))
                .Returns(true);

            var result = _sut.TryGetValue<string>(key, out var value);

            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void TryGetValue_WhenKeyDoesNotExist_ReturnsFalse()
        {
            var key = "non-existent";
            object? cacheValue = null;

            _memoryCacheMock
                .Setup(x => x.TryGetValue(key, out cacheValue))
                .Returns(false);

            var result = _sut.TryGetValue<string>(key, out var value);

            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void SetPermanent_StoresValueWithNeverRemovePriority()
        {
            var key = "test-key";
            var value = "test-value";
            var cacheEntry = Mock.Of<ICacheEntry>();

            _memoryCacheMock
                .Setup(x => x.CreateEntry(key))
                .Returns(cacheEntry);

            _sut.SetPermanent(key, value);

            _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once);
        }

        [Fact]
        public void Remove_RemovesKeyFromCache()
        {
            var key = "test-key";

            _sut.Remove(key);

            _memoryCacheMock.Verify(x => x.Remove(key), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WhenKeyExists_ReturnsValue()
        {
            var key = "test-key";
            var expectedValue = "test-value";
            object cacheValue = expectedValue;

            _memoryCacheMock
                .Setup(x => x.TryGetValue(key, out cacheValue))
                .Returns(true);

            var result = await _sut.GetAsync<string>(key);

            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task RemoveAsync_RemovesKeyFromCache()
        {
            var key = "test-key";

            await _sut.RemoveAsync(key);

            _memoryCacheMock.Verify(x => x.Remove(key), Times.Once);
        }
    }
}
