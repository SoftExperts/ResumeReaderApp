using System.Text;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using SimpleNetNlp;

namespace ResumeReaderApp
{
    public class Program
    {
        public static void Main(string[] args)
        {

            string filePath = @"C:\Users\Asif\Downloads\CV\Resume2.pdf";

            // Get all text from the Resume.
            var text = GetAllTextFromResume(filePath);

            // Created resume text into one sentence.
            var sentence = new Sentence(text);

            // Get all the Index of Common Attributes like First Name, Last Name, Email and Phone Etc.
            var indexesOfAttributes =  GetIndexesOfCommonAttributes(sentence);

            // Get the all Words in the Sentence.
            var getAllWords = sentence.Words.ToList();

            var filteredData = GetFilteredData(indexesOfAttributes, getAllWords);

            // This model provide the relations between sentence based on subject, relation and object
            var openIe = sentence.OpenIe();
            
            // This model provide each token in the sentence.
            var lem = sentence.Lemmas();

            // This model tells the sentence is positive or negative.
            var sent = sentence.Sentiment();

            // Returns the part of speech tags of the sentence, one for each token in the sentence.
            var post = sentence.PosTags();
            
            //
            var gov = sentence.Governors();

            Console.WriteLine("sentence:{}");


        }

        private static ResumInfo GetFilteredData(DataIndexesInfo indexesOfAttributes, List<string> getAllWords)
        {
            try
            {
                string[] getPersonName = new string[indexesOfAttributes.PersonsIndexes.Length];

                string[] getPhoneNumbers = new string[indexesOfAttributes.PhoneIndexes.Length];
                
                string[] getOrganizations = new string[indexesOfAttributes.OrgIndexes.Length];

                for (int i = 0; i < indexesOfAttributes.PersonsIndexes.Length; i++)
                {
                    int index = indexesOfAttributes.PersonsIndexes[i];
                    if (index >= 0 && index < getAllWords.Count)
                    {
                        getPersonName[i] = getAllWords[index];
                    }
                }
                
                for (int i = 0; i < indexesOfAttributes.PhoneIndexes.Length; i++)
                {
                    int index = indexesOfAttributes.PhoneIndexes[i];
                    if (index >= 0 && index < getAllWords.Count)
                    {
                        getPhoneNumbers[i] = getAllWords[index];
                    }
                }
                
                for (int i = 0; i < indexesOfAttributes.OrgIndexes.Length; i++)
                {
                    int index = indexesOfAttributes.OrgIndexes[i];
                    if (index >= 0 && index < getAllWords.Count)
                    {
                        getOrganizations[i] = getAllWords[index];
                    }
                }

                ResumInfo resumInfo = new ResumInfo();

                resumInfo.FirstName = getPersonName.Length > 0? getPersonName[0] : null;
               
                resumInfo.MiddleName = getPersonName.Length > 0? getPersonName[1] : null;
                
                resumInfo.LastName = getPersonName.Length > 0? getPersonName[2] : null;
                               
                resumInfo.Email = indexesOfAttributes.EmailIndex > 0 ? getAllWords[indexesOfAttributes.EmailIndex] : null;
               
                resumInfo.City = indexesOfAttributes.CityIndex > 0 ? getAllWords[indexesOfAttributes.CityIndex] : null;
                
                resumInfo.Country = indexesOfAttributes.CountryIndex > 0 ? getAllWords[indexesOfAttributes.CountryIndex] : null;
                
                resumInfo.PhoneNumbers = getPhoneNumbers.Length > 0? getPhoneNumbers.ToList() : null;
                
                resumInfo.Organizations = getOrganizations.Length > 0? getOrganizations.ToList() : null;
                
                return resumInfo;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static string GetAllTextFromResume(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Read the PDF file into a byte array
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    // Create an IFormFile instance from the byte array
                    IFormFile pdfFile = new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", System.IO.Path.GetFileName(filePath));

                    return ReadPdfFile(pdfFile);
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static DataIndexesInfo GetIndexesOfCommonAttributes(Sentence? sentence)
        {
            try
            {
                DataIndexesInfo dataIndexInfo = new DataIndexesInfo();

                var tags = sentence?.NerTags()?.ToList();

                dataIndexInfo.PersonsIndexes = Enumerable.Range(0, tags.ToArray().Length)
                                  .Where(i => tags[i] == "PERSON")
                                  .ToArray();

                dataIndexInfo.PhoneIndexes = Enumerable.Range(0, tags.ToArray().Length)
                                  .Where(i => tags[i] == "NUMBER")
                                  .ToArray();

                dataIndexInfo.OrgIndexes = Enumerable.Range(0, tags.ToArray().Length)
                                  .Where(i => tags[i] == "ORGANIZATION")
                                  .ToArray();

                dataIndexInfo.EmailIndex = tags.IndexOf("EMAIL");

                dataIndexInfo.CityIndex = tags.IndexOf("CITY");

                dataIndexInfo.CountryIndex = tags.IndexOf("COUNTRY") ;

                return dataIndexInfo;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static string ReadPdfFile(IFormFile resumeFile)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    resumeFile.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new PdfReader(memoryStream))
                    {
                        var text = new StringBuilder();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            var pageText = PdfTextExtractor.GetTextFromPage(reader, i);
                            text.Append(pageText);
                        }

                        return text.ToString();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}

public class DataIndexesInfo
{
    public int[]? PersonsIndexes { get; set; }
    public int EmailIndex { get; set; }
    public int CityIndex { get; set; }
    public int CountryIndex { get; set; }
    public int[]? OrgIndexes { get; set; }
    public int[]? PhoneIndexes { get; set; }
}

public class ResumInfo
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public List<string>? PhoneNumbers { get; set; }
    public List<string>? Organizations { get; set; }
}