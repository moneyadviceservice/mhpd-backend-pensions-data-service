using PensionsDataService.Utilities;

namespace PensionsDataServiceUnitTests.Utilities;

public sealed class TimelineSeriesBuilderTests
{
    private readonly TimelineSeriesBuilder _builder;

    public TimelineSeriesBuilderTests()
    {
        var navigator = new PensionNavigator();
        var factory = new TimelineArrangementFactory(navigator);
        _builder = new TimelineSeriesBuilder(factory);
    }

    [Fact]
    public void Build_Generates_Only_Years_Of_Change()
    {
        var pensions = TestData.CreateTwoOverlappingPensions();

        var series = _builder.Build(pensions);

        Assert.Contains(series.Years, y => y.Year == 2030);
        Assert.Contains(series.Years, y => y.Year == 2035);
        Assert.DoesNotContain(series.Years, y => y.Year == 2031);
    }

    [Fact]
    public void LumpSum_Pension_Does_Not_Contribute_To_Totals()
    {
        var pensions = TestData.CreatePensionsContainingLumpSum();

        var series = _builder.Build(pensions);

        Assert.DoesNotContain(series.Years, year => year.AnnualTotal > 12000);
    }

    [Fact]
    public void Keys_Contain_LU_When_Any_LumpSum_Present()
    {
        var pensions = TestData.CreatePensionsContainingLumpSum();

        var series = _builder.Build(pensions);

        Assert.Contains("LU", series.Keys);
        Assert.Equal("LU", series.Keys.Last());
    }

    [Fact]
    public void Build_Generates_TimeSeries_ForPensions()
    {
        var pensions = TestData.CreateMultiplePensions();

        var series = _builder.Build(pensions);

        Assert.Equal(4, series.Keys.Count);
        Assert.Contains(series.Years, y => y.Year == 2030);
        Assert.Contains(series.Years, y => y.Year == 2035);
        Assert.Contains(series.Years, y => y.Year == 2061);
        Assert.DoesNotContain(series.Years, y => y.Year == 2060);
        Assert.Equal("SP", series.Keys[0]);
        Assert.Equal("LU", series.Keys.Last());
    }
}
