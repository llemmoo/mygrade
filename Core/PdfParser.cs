using UglyToad.PdfPig;
using Core.Model;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Core;

public class PdfParser
{
    public List<ClassGrade> GetClassGradeFromPdf(string inputPath)
    {
        List<ClassGrade> classGrades = [];
        var pageNumber = 1;
        using (var document = PdfDocument.Open(inputPath))
        {
            var builder = new PdfDocumentBuilder { };
            PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
            var pageBuilder = builder.AddPage(document, pageNumber);
            pageBuilder.SetStrokeColor(0, 255, 0);
            var page = document.GetPage(pageNumber);

            var letters = page.Letters; // no preprocessing
            
            var wordExtractor = NearestNeighbourWordExtractor.Instance;
            var words = wordExtractor.GetWords(letters);
            
            var pageSegmenter = DocstrumBoundingBoxes.Instance;
            var textBlocks = pageSegmenter.GetBlocks(words);

            // 3. Postprocessing
            var readingOrder = UnsupervisedReadingOrderDetector.Instance;
            var orderedTextBlocks = readingOrder.Get(textBlocks);
            var listOfBlocks = orderedTextBlocks.ToList();
            
            //4. Grades
            var gradeIndexStart = listOfBlocks.FindIndex(b => b.Text.Contains("Marks"))+1;
            var gradeIndexEnd = listOfBlocks.FindIndex(b => b.Text.Contains("ECTS-\nscale"));
            var gradeBlocks = listOfBlocks.GetRange(gradeIndexStart, gradeIndexEnd-gradeIndexStart);
            List<int> grades = [];

            foreach (var gradeBlock in gradeBlocks)
            {
                var gradeLines = gradeBlock.Text.Split('\n');
                foreach (var gradeLine in gradeLines)
                {
                    if (gradeLine.ToLower() == "pass" || gradeLine.ToLower() == "passed")
                    {
                        grades.Add(0);
                    }
                    else
                    {
                        grades.Add(int.Parse(gradeLine));
                    }
                }
            }
            
            //5. Credits
            //Credits are the last column, as such we will simply go to the max of the range.
            var creditsIndexStart = listOfBlocks.FindIndex(b => b.Text.Contains("1 of"))+1;
            
            var creditBlocks = listOfBlocks.GetRange(creditsIndexStart, listOfBlocks.Count-creditsIndexStart);
            List<int> credits = [];
            
            foreach (var creditBlock in creditBlocks)
            {
                var creditLines = creditBlock.Text.Split('\n');
                foreach (var creditLine in creditLines)
                {
                    if (!char.IsNumber(creditLine[0])) continue;
                    var creditNumber = int.Parse(creditLine);
                    if (creditNumber > 20 || creditNumber < 5) continue;
                    credits.Add(creditNumber);
                }
            }

            if (grades.Count() != credits.Count()) throw new Exception("Grade/Credit count missmatch");
            
            for (int i = 0; i < grades.Count; i++)
            {
                if (grades[i] == 0) continue;
                classGrades.Add(new ClassGrade { Name = i.ToString(), Grade = grades[i], Weight = credits[i]});
            }
        }
        return classGrades;
    }

    public void CreateDebugPdf(string inputPath, string outputPath)
    {
        var pageNumber = 1;
        using (var document = PdfDocument.Open(inputPath))
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
    
    public decimal GetWeightedResult(List<ClassGrade> classGrades)
    {
        //Calculate weighted grades
        var gradeSum = classGrades.Sum(cg => cg.Grade * cg.Weight);
        var creditSum = (decimal)classGrades.Sum(cg => cg.Weight);

        return Math.Round(gradeSum / creditSum, 2);
    }
    
}