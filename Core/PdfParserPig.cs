using UglyToad.PdfPig;
using Core.Model;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Core;

public class PdfParserPig
{
    
    public PdfParserPig()
    {
    }

    public List<ClassGrade> GetClassGradeFromPdf(string pdfPath)
    {
        List<ClassGrade> classGrades = [];
        using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(pdfPath))
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

    public void CreateDebugPdf(string inputPath, string outputPath)
    {
        var pageNumber = 1;
        using (var document = UglyToad.PdfPig.PdfDocument.Open(inputPath))
        {
            var builder = new PdfDocumentBuilder { };
            PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
            var pageBuilder = builder.AddPage(document, pageNumber);
            pageBuilder.SetStrokeColor(0, 255, 0);
            var page = document.GetPage(pageNumber);

            var letters = page.Letters; // no preprocessing

            // 1. Extract words
            var wordExtractor = NearestNeighbourWordExtractor.Instance;

            var words = wordExtractor.GetWords(letters);

            // 2. Segment page
            var pageSegmenter = DocstrumBoundingBoxes.Instance;

            var textBlocks = pageSegmenter.GetBlocks(words);

            // 3. Postprocessing
            var readingOrder = UnsupervisedReadingOrderDetector.Instance;
            var orderedTextBlocks = readingOrder.Get(textBlocks);

            // 4. Add debug info - Bounding boxes and reading order
            foreach (var block in orderedTextBlocks)
            {
                var bbox = block.BoundingBox;
                pageBuilder.DrawRectangle(bbox.BottomLeft, bbox.Width, bbox.Height);
                pageBuilder.AddText(block.ReadingOrder.ToString(), 8, bbox.TopLeft, font);
            }

            // 5. Write result to a file
            byte[] fileBytes = builder.Build();
            File.WriteAllBytes(outputPath, fileBytes); // save to file
        }
    }
    
    public decimal GetWeightGradeResult(List<ClassGrade> classGrades)
    {
        return 0.0m;
    }
    
}