using System.Text.RegularExpressions;
namespace MoogleEngine;


public static class Moogle
{
    //Properties
    private static string BD_Route = @"C:\Users\elsingon\source\repos\moogle\Content";
    private static Documents documents = new Documents(BD_Route);
  
    //Methods
    public static SearchResult Query(string query)
    {
        // Modifique este método para responder a la búsqueda
        Query userInput = new Query(query, documents.WordSet, documents.IDF);
        float[] scores = GetScores(documents.TF_IDF, userInput.TF_IDF);
        SearchItem[] items = new SearchItem[3]
        {
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", scores[0]),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.5f),
            new SearchItem("Hello World", "Lorem ipsum dolor sit amet", 0.1f),
        };
        
        return new SearchResult(items, query);
    }
    // Returns an array of floats in which every element represents a score in range [0,1]
    private static float[] GetScores(float[,] documentsTF_IDF, float[] queryTF_IDF)
    {
        float[] result = new float[queryTF_IDF.Length];
        Vector queryVector = new Vector(queryTF_IDF);
        for (int i = 0; i <documentsTF_IDF.GetLength(0); i++)
        {
            float[] doc = new float[documentsTF_IDF.GetLength(0)];
            for (int j = 0; j < documentsTF_IDF.GetLength(1); j++)
            {
                doc[i] = documentsTF_IDF[i,j];
            }
            Vector docVecor = new Vector(doc);
            result[i] = Vector.Cos(queryVector, docVecor);
        }
        return result;
    }
    
    
}
public class Documents
{
    //Instance Variables
    private string DirRoute;
    private string[] FilesRoutes;
    private Dictionary<string, Dictionary<string, int>> docTF;
    private string[] wordset;
    private int[,] tf;
    private float[] idf;
    private float[,] tf_idf;
    //Properties
    private Dictionary<string, Dictionary<string, int>> DocTF //TF of each document
        {
            get
            {
                return docTF;
            }
        }
    public string[] WordSet //Word Set Propertie
        {
            get
            {

                return wordset;
            }
        }
    private int[,] TF
        {
            get
            {
                return tf;
            }
        } //Term Absolute Frequency
    //Public Properties
    public float[] IDF
        {
            get
            {
               return idf;
            }
        } 
    public float[,] TF_IDF
        {
            get
            { 
                return tf_idf;
            }
        }
    //Methods
    public Documents(string DirRoute) //Constructor
        {
            this.DirRoute = DirRoute;
            this.FilesRoutes = Directory.GetFiles(DirRoute, "*.txt");
            this.docTF = ComputeDocTF();
            this.wordset = GetWordSet();
            this.tf = GetTF();
            this.idf = GetIDF();
            this.tf_idf = GetTF_IDF();
        }
    private Dictionary<string, Dictionary<string, int>> ComputeDocTF() //Returns the TF of each document
        {
            Dictionary<string, Dictionary<string, int>> result = new Dictionary<string, Dictionary<string, int>>();
            foreach (var FileRoute in this.FilesRoutes)
            {
                Dictionary<string, int> Doc = new Dictionary<string, int>();
                foreach (var word in TokenizeDocument(FileRoute))
                {
                    if (!Doc.ContainsKey(word))
                    {
                        Doc[word.ToLower()] = 1;
                    }
                    else
                    {
                        Doc[word]++;
                    }
                }
                result.Add(Path.GetFileName(FileRoute), Doc);
            }
            return result;
        }
    public string[] GetWordSet() // Returns the WordSet of a Universe of Documents
        {
            HashSet<string> result = new HashSet<string>();
            foreach (KeyValuePair<string, Dictionary<string, int>> Doc in this.DocTF)
            {
                foreach (string word in Doc.Value.Keys)
                {
                    result.Add(word);

                }
            }
            return result.ToArray();
        }
    private string[] TokenizeDocument(string DocRoute) // Tokenize a Document
        {
            string CurrentDocument = File.ReadAllText(DocRoute);
            string[] words = Regex.Split(CurrentDocument, @"\W+").Where(elem=>elem!=" " && elem!="").ToArray();
            return words;
        }
    private int[,] GetTF()
        {
            int[,] result = new int[this.WordSet.Length, DocTF.Count];
            for (int wordIndex = 0; wordIndex < this.WordSet.Length; wordIndex++)
            {
                int counter = 0;
                foreach (Dictionary<string, int> keyValuePairs in this.DocTF.Values)
                {
                    if (keyValuePairs.ContainsKey(this.WordSet[wordIndex]))
                    {
                        result[wordIndex, counter] = keyValuePairs[this.WordSet[wordIndex]];
                    }
                    counter++;
                }
            }
            return result;
        }
    private float [,] GetTF_IDF()
        {
            float [,] result = new float[this.TF.GetLength(0),this.TF.GetLength(1)];
            for (int i = 0; i < this.TF.GetLength(0); i++)
            {
                float scalar = this.IDF[i];
                for (int j = 0; j < this.TF.GetLength(1); j++)
                {
                    result[i,j] = scalar*TF[i,j];
                }
            }
            return result;
        }
    public float[] GetIDF() //Returns the Documents IDF
        {
            float[] idf = new float[this.TF.GetLength(0)];
            int numberOfDocuments = this.TF.GetLength(1);
            //  Console.WriteLine("Numero de Documentos: "+numberOfDocuments);
            for (int i = 0; i < this.TF.GetLength(0); i++)
            {
                int DocumentsContainingTerm = 0;
                for (int j = 0; j < this.TF.GetLength(1); j++)
                {
                    if (this.TF[i, j] != 0)
                    {
                        DocumentsContainingTerm++;
                    }
                }
                // Console.WriteLine("Numero de documentos que contienen al termino");
                idf[i] = (float)Math.Log10((float)numberOfDocuments / DocumentsContainingTerm);
            }
            return idf;
        }
    //Void Methods
    public void PrintDocTF() //Prints each document and its word's TF
        {
            foreach(KeyValuePair<string,Dictionary<string,int>> doc in this.DocTF)
            {
                Console.WriteLine("####################################################################################################");
                Console.WriteLine("Document: "+ doc.Key);
                Console.WriteLine("\n");
                foreach (KeyValuePair<string,int> word in doc.Value)
                {
                    Console.WriteLine("Word: "+word.Key+" Frecuency: "+  word.Value);
                }
            }
            Console.WriteLine("####################################################################################################");
        }
    public void PrintWordSet()
        {
            foreach(string word in this.GetWordSet())
            {
                Console.WriteLine(word);
            }
        }//Prints the WordSet of the Documents's Universe
    public void PrintTF()
        {
            for(int i = 0;i<this.GetTF().GetLength(0);i++)
            {
                for(int j = 0;j<this.GetTF().GetLength(1);j++)
                {
                    Console.Write(GetTF()[i,j]+" ");
                }
                Console.WriteLine();
            }
        }//Prints each word's absolute frequency on the Document's Universe
}
public class Vector
{
    //Properties
    private float[] components;
    private int dimension;
    private float Module
    {
        get
        {
            float Sum = 0;
            foreach (var component in components)
            {
                Sum += (float)Math.Pow(component, 2);
            }
            return (float)Math.Sqrt(Sum);
        }
    }
    //Methods
    public Vector(float[] components) //Constructor
    {
        this.components = components;
        this.dimension = this.components.Length;
    }
    private static float DotProduct(Vector v1, Vector v2)
    {
        float res = 0;
        for (int i = 0; i < v1.dimension; i++)
        {
            res += v1.components[i] * v2.components[i];
        }
        return res;
    }
    public static float Cos(Vector v1, Vector v2)
    {
        return DotProduct(v1, v2) / (v1.Module * v2.Module);
    }

}
public class Query
{

    //Instance Variables
    private string query;
    private string[] WordSet;
    private float[] IDF;
    private string[] tokenizedquery;
    private int[] tf;
    private float[] tf_idf;

    //Properties
    private string[] TokenizedQuery
    {
        get
        {
            return this.tokenizedquery;
        }
    }
    private int[] TF
    {
        get
        {
            return this.tf;
        }
    }
    public float[] TF_IDF
    {
        get
        {
            return tf_idf;
        }
    }



    //Methods 
    public Query(string query, string[] WordSet, float[] IDF) //Constructor
    {
        this.query = query;
        this.WordSet = WordSet;
        this.IDF = IDF;
        this.tokenizedquery = GetTokenizedQuery();
        this.tf = GetTF();
        this.tf_idf = GetTF_IDF();
    }
    private string[] GetTokenizedQuery()
    {
        return Regex.Split(this.query, @"\W+").Where(elem => elem != " " && elem != "").ToArray();
    }
    public int[] GetTF()
    {
        int[] result = new int[WordSet.Length];
        for (int i = 0; i < WordSet.Length; i++)
        {
            if (this.TokenizedQuery.Contains(WordSet[i].ToLower()))
            {
                result[i] = this.TokenizedQuery.Count(word => word == WordSet[i]);
            }
        }
        return result;
    }

    public float[] GetTF_IDF()
    {
        float[] result = new float[this.TF.Length];
        for (int i = 0; i < this.TF.Length; i++)
        {
            result[i] = this.TF[i] * this.IDF[i];
        }
        return result;
    }
}



