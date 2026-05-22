using System.Collections.Generic;
using Kombinle.Core.Domain;
using Kombinle.Core.Scoring.Presenting;
using Xunit;

namespace Kombinle.Core.Tests
{
    public class WardrobeFeedbackPresenterTests
    {
        //[Fact]
        //public void When_ContextMessageExists_ShouldUseCatalogTitleAndDetail()
        //{
        //    // Arrange
        //    var feedback = new List<WardrobeFeedback>
        //    {
        //        new WardrobeFeedback(
        //            WardrobeGapType.MissingItemForContext,
        //            "RAIN_SUEDE_SHOES",
        //            "debug"
        //        )
        //    };

        //    // Act
        //    var ux = WardrobeFeedbackPresenter.BuildUx(feedback);

        //    // Assert
        //    Assert.NotNull(ux);
        //    Assert.Contains("Yağmurda süet ayakkabı riskli", ux!.LineTr);
        //    Assert.NotNull(ux.DetailTr);
        //    Assert.Contains("Süet malzeme suyu çeker", ux.DetailTr!);
        //}

        //[Fact]
        //public void When_ContextMessageDoesNotExist_ShouldFallbackToCode()
        //{
        //    // Arrange
        //    var feedback = new List<WardrobeFeedback>
        //    {
        //        new WardrobeFeedback(
        //            WardrobeGapType.MissingItemForContext,
        //            "UNKNOWN_CONTEXT_CODE",
        //            "debug"
        //        )
        //    };

        //    // Act
        //    var ux = WardrobeFeedbackPresenter.BuildUx(feedback);

        //    // Assert
        //    Assert.NotNull(ux);
        //    Assert.Contains("UNKNOWN_CONTEXT_CODE", ux!.LineTr);
        //    Assert.Null(ux.DetailTr);
        //}

        //[Fact]
        //public void When_NoWardrobeFeedback_ShouldReturnNull()
        //{
        //    // Arrange
        //    var feedback = new List<WardrobeFeedback>();

        //    // Act
        //    var ux = WardrobeFeedbackPresenter.BuildUx(feedback);

        //    // Assert
        //    Assert.Null(ux);
        //}
    }
}
