using Core;
using Xunit;

namespace Test;

public class PdfParserTests
{
    private PdfParser _pdfParser;
    public PdfParserTests()
    {
        _pdfParser = new PdfParser();
    }

    [Fact]
    public void ClassGradesShouldNotBeNull()
    {
        var classGrades = _pdfParser.GetClassGradeFromPdf(@"C:\Users\Oliver\Downloads\Grades.pdf");
        Assert.NotNull(classGrades);
    }

    [Fact]
    public void WeightedAverageShouldBeCorrect()
    {
        var classGrades = _pdfParser.GetClassGradeFromPdf(@"C:\Users\Oliver\Downloads\Grades.pdf");
        var average = _pdfParser.GetWeightedResult(classGrades);

        Assert.InRange(average, 0,13);
    }
    
    [Fact]
    public void CreateDebugPdf()

    {
        _pdfParser.CreateDebugPdf(@"C:\Users\Oliver\Downloads\Grades.pdf", @"C:\Users\Oliver\Documents\DebugGrades.pdf");
    }
}