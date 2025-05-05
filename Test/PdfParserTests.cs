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
}