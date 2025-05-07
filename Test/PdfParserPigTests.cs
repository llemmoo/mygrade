using Core;
using Xunit;

namespace Test;

public class PdfParserPigTests
{
    private PdfParserPig _pdfParserPig;
    public PdfParserPigTests()
    {
        _pdfParserPig = new PdfParserPig();
    }

    [Fact]
    public void ClassGradesShouldNotBeNull()

    {
        var classGrades = _pdfParserPig.GetClassGradeFromPdf(@"C:\Users\Oliver\Downloads\Grades.pdf");
        Assert.NotNull(classGrades);
    }
    
    [Fact]
    public void CreateDebugPdf()

    {
        _pdfParserPig.CreateDebugPdf(@"C:\Users\Oliver\Downloads\Grades.pdf", @"C:\Users\Oliver\Documents\DebugGrades.pdf");
    }
}