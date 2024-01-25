using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Data.Sqlite;

using MathNet.Numerics.LinearAlgebra.Double;

namespace PulsenicsApp
{
  // Definition of a Point class to represent coordinates in a two-dimensional space
  public class Point
  {
    // Property representing the X-coordinate of the point
    public double X { get; set; }

    // Property representing the Y-coordinate of the point
    public double Y { get; set; }
  }

}

namespace PulsenicsApp
{
  public class IndexModel : PageModel
  {
    private readonly ILogger<IndexModel> _logger;

    // Constructor to initialize the logger
    public IndexModel(ILogger<IndexModel> logger)
    {
      _logger = logger;
    }

    public class Point
    {
      public double X { get; set; }
      public double Y { get; set; }
    }

    // BindProperty for input points
    [BindProperty]

    public List<Point> Points { get; set; }

    // BindProperty for selected curve type
    [BindProperty]
    public string CurveType { get; set; }

    // Property to store the fitted equation
    public string FittedEquation { get; private set; }

    // Model class to represent the result of curve fitting
    public class CurveFittingResult
    {
      public string FittedEquation { get; set; }
      public string CurveType { get; set; }
      public List<Point> Points { get; set; }
      public List<Point> DatabaseData { get; set; }
    }

    // Handler for HTTP GET request
    public async Task OnGetAsync()
    {
      // Initialize default values for points and curve type
      InitializeDefaultValues();

      // Process points and retrieve fitted equation
      FittedEquation = ProcessPoints();

      // Fetch data from SQLite database asynchronously
      var databaseData = await GetDataFromSQLiteAsync();

      // Log data from SQLite Database
      Console.WriteLine("Data from SQLite Database:", JsonConvert.SerializeObject(databaseData));
    }

    // Handler for HTTP POST request to process points
    public IActionResult OnPostProcessPoints()
    {
      // Process points and retrieve fitted equation
      FittedEquation = ProcessPoints();

      // Save data to SQLite database
      SaveDataToSQLite(Points, CurveType, FittedEquation);

      // Fetch data from SQLite database
      var databaseData = GetDataFromSQLiteAsync().Result;

      // Create result object for JSON response
      var result = new CurveFittingResult
      {
        FittedEquation = FittedEquation,
        CurveType = CurveType,
        Points = Points,
        DatabaseData = databaseData
      };

      // Return JSON result
      return new JsonResult(result);
    }

    // Method to initialize default values for Points and CurveType
    private void InitializeDefaultValues()
    {
      Points = new List<Point>
      {
        new Point(),
        new Point()
      };
      CurveType = "linear";
    }

    // Method to process points based on the selected CurveType
    private string ProcessPoints()
    {
      try
      {
        // Switch statement to handle different curve types
        switch (CurveType.ToLower())
        {
          case "linear":
            var (linearSlope, linearIntercept) = FitLinearCurve(Points);
            return GetLinearEquation(linearSlope, linearIntercept);

          case "quadratic":
            var (quadA, quadB, quadC) = FitQuadraticCurve(Points);
            return GetQuadraticEquation(quadA, quadB, quadC);

          case "cubic":
            var (cubicD, cubicA, cubicB, cubicC) = FitCubicCurve(Points);
            return GetCubicEquation(cubicD, cubicA, cubicB, cubicC);

          default:
            return string.Empty;
        }
      }
      catch (Exception ex)
      {
        // Log error if any exception occurs
        _logger.LogError(ex, "Error processing points");
        return "Error";
      }
    }

    // Method to fit a linear curve to the given points
    private (double Slope, double Intercept) FitLinearCurve(List<Point> points)
    {
      int n = points.Count;
      double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

      // Calculate sums required for linear regression
      foreach (var point in points)
      {
        sumX += point.X;
        sumY += point.Y;
        sumXY += point.X * point.Y;
        sumX2 += point.X * point.X;
      }

      // Create a matrix and vector, then solve for coefficients
      var matrixA = DenseMatrix.OfArray(new double[,] { { n, sumX }, { sumX, sumX2 } });
      var vectorB = DenseVector.OfArray(new double[] { sumY, sumXY });

      var coefficients = matrixA.Solve(vectorB);

      return (coefficients[1], coefficients[0]);
    }

    // Method to get the linear equation based on slope and intercept
    private string GetLinearEquation(double slope, double intercept)
    {
      return $"y = {slope}x + {intercept}";
    }

    // Method to fit a quadratic curve to the given points
    private (double A, double B, double C) FitQuadraticCurve(List<Point> points)
    {
      int n = points.Count;
      double sumX = 0, sumY = 0, sumX2 = 0, sumX3 = 0, sumX4 = 0, sumXY = 0, sumX2Y = 0;

      // Calculate sums required for quadratic regression
      foreach (var point in points)
      {
        double x2 = point.X * point.X;
        sumX += point.X;
        sumY += point.Y;
        sumX2 += x2;
        sumX3 += x2 * point.X;
        sumX4 += x2 * x2;
        sumXY += point.X * point.Y;
        sumX2Y += x2 * point.Y;
      }

      // Create a matrix and vector, then solve for coefficients
      var matrixA = DenseMatrix.OfArray(new double[,] { { n, sumX, sumX2 }, { sumX, sumX2, sumX3 }, { sumX2, sumX3, sumX4 } });
      var vectorB = DenseVector.OfArray(new double[] { sumY, sumXY, sumX2Y });

      var coefficients = matrixA.Solve(vectorB);

      return (coefficients[2], coefficients[1], coefficients[0]);
    }

    // Method to get the quadratic equation based on coefficients A, B, and C
    private string GetQuadraticEquation(double A, double B, double C)
    {
      return $"y = {A}x^2 + {B}x + {C}";
    }

    // Method to fit a cubic curve to the given points
    private (double A, double B, double C, double D) FitCubicCurve(List<Point> points)
    {
      int n = points.Count;
      double sumX = 0, sumY = 0, sumX2 = 0, sumX3 = 0, sumX4 = 0, sumX5 = 0, sumX2Y = 0, sumX3Y = 0;

      // Calculate sums required for cubic regression
      foreach (var point in points)
      {
        double x2 = point.X * point.X;
        double x3 = x2 * point.X;
        sumX += point.X;
        sumY += point.Y;
        sumX2 += x2;
        sumX3 += x3;
        sumX4 += x2 * x2;
        sumX5 += x3 * point.X;
        sumX2Y += x2 * point.Y;
        sumX3Y += x3 * point.Y;
      }

      // Create a matrix and vector, then solve for coefficients
      var matrixA = DenseMatrix.OfArray(new double[,] { { n, sumX, sumX2, sumX3 }, { sumX, sumX2, sumX3, sumX4 }, { sumX2, sumX3, sumX4, sumX5 }, { sumX3, sumX4, sumX5, sumX5 * sumX } });
      var vectorB = DenseVector.OfArray(new double[] { sumY, sumX2Y, sumX3Y, sumX4 * sumY });

      var coefficients = matrixA.Solve(vectorB);

      return (coefficients[3], coefficients[2], coefficients[1], coefficients[0]);
    }

    // Method to get the cubic equation based on coefficients A, B, C, and D
    private string GetCubicEquation(double A, double B, double C, double D)
    {
      return $"y = {A}x^3 + {B}x^2 + {C}x + {D}";
    }

    // Method to save points, curve type, and fitted equation to SQLite database
    private void SaveDataToSQLite(List<Point> points, string curveType, string fittedEquation)
    {
      try
      {
        // Construct the path to the SQLite database file
        string databasePath = Path.Combine(Directory.GetCurrentDirectory(), "mydatabase.sqlite");
        string connectionString = $"Data Source={databasePath}";

        // Open a connection to the SQLite database
        using (var connection = new SqliteConnection(connectionString))
        {
          connection.Open();

          // Create the CurveData table if it doesn't exist
          using (var command = new SqliteCommand("CREATE TABLE IF NOT EXISTS CurveData (X REAL, Y REAL, CurveType TEXT, FittedEquation TEXT)", connection))
          {
            command.ExecuteNonQuery();
          }

          // Insert each point, curve type, and fitted equation into the CurveData table
          foreach (var point in points)
          {
            using (var command = new SqliteCommand("INSERT INTO CurveData (X, Y, CurveType, FittedEquation) VALUES (@X, @Y, @CurveType, @FittedEquation)", connection))
            {
              command.Parameters.AddWithValue("@X", point.X);
              command.Parameters.AddWithValue("@Y", point.Y);
              command.Parameters.AddWithValue("@CurveType", curveType);
              command.Parameters.AddWithValue("@FittedEquation", fittedEquation);
              command.ExecuteNonQuery();
            }
          }
        }
      }
      catch (Exception ex)
      {
        // Log error if any exception occurs during database operation
        _logger.LogError(ex, "Error saving data to SQLite database");
      }
    }

    // Get data asynchronously from the SQLite database
    private async Task<List<Point>> GetDataFromSQLiteAsync()
    {
      // Create a list to store the retrieved data points
      var data = new List<Point>();

      try
      {
        // Set up the connection string for SQLite database
        string connectionString = "Data Source=mydatabase.sqlite";

        // Create and open a connection to the SQLite database asynchronously
        using (var connection = new SqliteConnection(connectionString))
        {
          await connection.OpenAsync();

          // Create a SQL command to select X and Y values from the 'CurveData' table
          using (var command = new SqliteCommand("SELECT X, Y FROM CurveData", connection))
          {
            // Execute the command asynchronously and obtain a data reader
            using (var reader = await command.ExecuteReaderAsync())
            {
              // Iterate through each row in the result set
              while (await reader.ReadAsync())
              {
                // Retrieve X and Y values from the current row and add a new Point to the list
                var x = reader.GetDouble(0);
                var y = reader.GetDouble(1);
                data.Add(new Point { X = x, Y = y });
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        // Log any errors that occur during the data retrieval process
        _logger.LogError(ex, "Error fetching data from SQLite database");
      }

      // Return the list of retrieved data points
      return data;
    }
  }
}
