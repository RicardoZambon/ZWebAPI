using Microsoft.EntityFrameworkCore.ChangeTracking;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.ExtensionMethods
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.ExtensionMethods.EntityEntryExtensions"/>.
    /// </summary>
    public class EntityEntryExtensionsTests
    {
        /// <summary>
        /// Test the HasValueModified should report false for an untouched property.
        /// </summary>
        [Fact]
        public void HasValueModified_Pass_FalseForAnUntouchedProperty()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            EntityFake entity = Track(context, "Alpha");

            // Act
            bool result = context.Entry(entity).Property(x => x.Name).HasValueModified();

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasValueModified should report true when the value changed.
        /// </summary>
        [Fact]
        public void HasValueModified_Pass_TrueWhenTheValueChanged()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            EntityFake entity = Track(context, "Alpha");
            entity.Name = "Beta";
            context.ChangeTracker.DetectChanges();

            // Act
            bool result = context.Entry(entity).Property(x => x.Name).HasValueModified();

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Test the HasValueModified should report true when a value was cleared.
        /// </summary>
        [Fact]
        public void HasValueModified_Pass_TrueWhenTheValueWasCleared()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            EntityFake entity = Track(context, "Alpha");
            entity.Name = null;
            context.ChangeTracker.DetectChanges();

            // Act
            bool result = context.Entry(entity).Property(x => x.Name).HasValueModified();

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Test the HasValueModified should report true when an empty value was filled in.
        /// </summary>
        [Fact]
        public void HasValueModified_Pass_TrueWhenAnEmptyValueWasFilledIn()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            EntityFake entity = Track(context, null);
            entity.Name = "Beta";
            context.ChangeTracker.DetectChanges();

            // Act
            bool result = context.Entry(entity).Property(x => x.Name).HasValueModified();

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Test the HasValueModified should report false when the property is flagged as modified but
        /// still holds the very same value.
        /// </summary>
        [Fact]
        public void HasValueModified_Pass_FalseWhenTheFlaggedValueDidNotChange()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            EntityFake entity = Track(context, "Alpha");
            PropertyEntry property = context.Entry(entity).Property(x => x.Name);
            property.IsModified = true;

            // Act
            bool result = property.HasValueModified();

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasValueModified should report false when the property is flagged as modified but
        /// was empty before and after.
        /// </summary>
        [Fact]
        public void HasValueModified_Pass_FalseWhenTheFlaggedValueStayedEmpty()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            EntityFake entity = Track(context, null);
            PropertyEntry property = context.Entry(entity).Property(x => x.Name);
            property.IsModified = true;

            // Act
            bool result = property.HasValueModified();

            // Assert
            result.Should().BeFalse();
        }

        private static EntityFake Track(DbContextFake context, string? name)
        {
            EntityFake entity = new() { ID = 1, Name = name };

            context.Entities.Add(entity);
            context.SaveChanges();

            return entity;
        }
    }
}
