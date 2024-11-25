using RepositoryStats.Core.Steps;

namespace RepositoryStats.Core.Test.Steps
{
    [TestFixture]
    public class CalculateFileStatisticsStepTests
    {
        private CalculateFileStatisticsStep _calculateFileStatisticsStep;

        [SetUp]
        public void Setup()
        {
            _calculateFileStatisticsStep = new CalculateFileStatisticsStep();
        }

        [Test]
        public async Task Execute_WhenFileContentIsValid_ShouldReturnSuccess()
        {
            // Arrange
            var fileContent = "function test() { return 42; }"u8.ToArray();
            
            // Act
            var result = await _calculateFileStatisticsStep.Execute(fileContent);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value['f'], Is.EqualTo(1));
            Assert.That(result.Value['u'], Is.EqualTo(2));
            Assert.That(result.Value.ContainsKey('a'), Is.False);
        }
        
        [Test]
        public async Task Execute_WhenFileContentContainsOtherCharacters_ShouldOnlyCountLetters()
        {
            // Arrange
            var fileContent = "function test() { return 42; }"u8.ToArray();
            
            // Act
            var result = await _calculateFileStatisticsStep.Execute(fileContent);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.ContainsKey('4'), Is.False);
            Assert.That(result.Value.ContainsKey('{'), Is.False);
        }

        [Test]
        public async Task Execute_WhenFileContentIsEmpty_ShouldReturnSuccess()
        {
            // Arrange
            var fileContent = Array.Empty<byte>();

            // Act
            var result = await _calculateFileStatisticsStep.Execute(fileContent);

            // Assert
            Assert.That(result.IsSuccess);
        }
        
        #pragma warning disable CS1998
        [Test]
        public async Task Execute_WhenFileContentIsNull_ShouldThrow()
        {
            // Arrange, Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => 
                await _calculateFileStatisticsStep.Execute(null!));
        }
        #pragma warning restore CS1998
    }
}