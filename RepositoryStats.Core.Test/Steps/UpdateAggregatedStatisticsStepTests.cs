using RepositoryStats.Core.Steps;

namespace RepositoryStats.Core.Test.Steps
{
    [TestFixture]
    public class UpdateAggregatedStatisticsStepTests
    {
        private UpdateAggregatedStatisticsStep _updateAggregatedStatistics;

        [SetUp]
        public void Setup()
        {
            _updateAggregatedStatistics = new UpdateAggregatedStatisticsStep();
        }
        
        [Test]
        public async Task Execute_WithTwoSetsOfStats_UpdatedAggregateCorrectly()
        {
            // Arrange
            var initialStatistics = BuildStatsSetOne();
            var updateStatistics = BuildStatsSetTwo();

            // Act
            await _updateAggregatedStatistics.Execute(initialStatistics);
            await _updateAggregatedStatistics.Execute(updateStatistics);
            var aggregated = _updateAggregatedStatistics.GetAggregatedStatistics();
            
            // Assert
            Assert.That(aggregated, Is.Not.Null);
            Assert.That(aggregated['B'] == 7);
            Assert.That(aggregated['f'] == 3);
            Assert.That(aggregated['Æ'] == 3);
        }

        [Test]
        public async Task Execute_WithEmptyDictionary_NoChangeToAggregate()
        {
            // Arrange
            var initialStatistics = BuildStatsSetOne();
            var emptyStats = new Dictionary<char, int>(); 

            // Act
            await _updateAggregatedStatistics.Execute(initialStatistics);
            await _updateAggregatedStatistics.Execute(emptyStats);
            var aggregated = _updateAggregatedStatistics.GetAggregatedStatistics();
            
            // Assert
            Assert.That(aggregated, Is.Not.Null);
            Assert.That(aggregated['B'] == 6);
            Assert.That(aggregated['Æ'] == 1);
        }
        
        private Dictionary<char, int> BuildStatsSetOne()
        {
            return new Dictionary<char, int>()
            {
                { 'a', 5 },
                { 'B', 6 },
                { 'c', 2 },
                { 'b', 3 },
                { 'Æ', 1 }
            };
        }
        
        private Dictionary<char, int> BuildStatsSetTwo()
        {
            return new Dictionary<char, int>()
            {
                { 'f', 3 },
                { 'B', 1 },
                { 'c', 3 },
                { 'b', 1 },
                { 'Æ', 2 },
                { 'æ', 2 }
            };
        }
    }
}