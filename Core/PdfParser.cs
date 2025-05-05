using UglyToad.PdfPig;
using Core.Model;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Core;

public class PdfParser
{
    
    public PdfParser()
    {
    }

    public List<ClassGrade> GetClassGradeFromPdf(string pdfPath)
    {
        List<ClassGrade> classGrades = [];
        using (PdfDocument document = PdfDocument.Open(pdfPath))
        {
            foreach (Page page in document.GetPages())
            {
                var words = page.GetWords().ToList();
                var groupedLines = words
                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1)) // adjust rounding if needed
                    .OrderByDescending(g => g.Key); // top to bottom
                foreach (var line in groupedLines)
                {
                    var lineWords = line.OrderBy(w => w.BoundingBox.Left).ToList();

                    if (lineWords.Count < 4)
                        continue;

                    // Assume last three words are Grade, American Grade, and Weight
                    string weight = lineWords[^1].Text;
                    string grade = lineWords[^3].Text;

                    // The rest is subject name
                    var subjectWords = lineWords.Take(lineWords.Count - 3).Select(w => w.Text);
                    string subject = string.Join(" ", subjectWords);
                    classGrades.Add(new ClassGrade
                    {
                        Name = subject,
                        Grade = int.Parse(grade),
                        Weight = int.Parse(weight)
                    });
                }
            }
        }
        return classGrades;
    }
    
    public decimal GetWeightGradeResult(List<ClassGrade> classGrades)
    {
        return 0.0m;
    }
    
}