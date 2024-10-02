using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class Product
{
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }

    public Product(string productName, string category, decimal price)
    {
        ProductName = productName;
        Category = category;
        Price = price;
    }

    private string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
    }

    public string Print()
    {
        string productLabel = "Product name:".PadRight(15);
        string categoryLabel = "Category:".PadRight(10);
        string priceLabel = "Price:".PadRight(7);
        string truncatedProductName = Truncate(ProductName, 20);
        string truncatedCategory = Truncate(Category, 15);

        return $"{productLabel} \x1B[31m{truncatedProductName.PadRight(20)}\x1B[0m {categoryLabel} \x1B[32m{truncatedCategory.PadRight(20)}\x1B[0m {priceLabel} \x1B[33m{Price:C}\x1B[0m";
    }
}

public class ProductManager
{
    private List<Product> products = new List<Product>();

    public void AddProduct(Product product)
    {
        bool isDuplicate = products.Any(p => p.ProductName.Equals(product.ProductName, StringComparison.OrdinalIgnoreCase) &&
                                             p.Category.Equals(product.Category, StringComparison.OrdinalIgnoreCase));

        if (!isDuplicate)
        {
            products.Add(product);
            Console.WriteLine($"\nProduct added successfully!");
        }
        else
        {
            Console.WriteLine($"A product with the name: \x1B[31m'{product.ProductName}'\x1B[0m  and category: \x1B[32m'{product.Category}'\x1B[0m already exists.");
        }
    }

    public void DisplayProducts(List<Product> productsToDisplay)
    {
        if (!productsToDisplay.Any())
        {
            Console.WriteLine("No products to display.");
            return;
        }

        var sortedProducts = productsToDisplay.OrderBy(p => p.Price).ToList();
        decimal totalPrice = sortedProducts.Sum(p => p.Price);

        Console.WriteLine("\nProducts \x1B[33m(Sorted by Price)\x1B[0m:\n");

        foreach (var product in sortedProducts)
        {
            Console.WriteLine(product.Print());
        }

        Console.WriteLine($"\nTotal Price of all products: \x1B[93m{totalPrice:C}\x1B[0m");
    }

    public void DisplayAllProducts()
    {
        DisplayProducts(products);
    }

    public List<Product> SearchProducts(string searchTerm)
    {
        return products
            .Where(p => p.ProductName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void SaveProductsToFile(string filePath)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(products);
            File.WriteAllText(filePath, jsonString);
            Console.WriteLine("Products successfully saved to file.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving products: \x1B[93m{ex.Message}\x1B[0m");
        }
    }

    public void LoadProductsFromFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                products = JsonSerializer.Deserialize<List<Product>>(jsonString) ?? new List<Product>();
                Console.WriteLine("\x1B[34mProducts successfully loaded from file! \x1B[0m");
                DisplayAllProducts();
            }
            else
            {
                Console.WriteLine("No previous product data found. Starting with an empty list.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products:  \x1B[93m{ex.Message}\x1B[0m");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        var productManager = new ProductManager();
        string filePath = "products.json";

        productManager.LoadProductsFromFile(filePath);
        bool continueRunning = true;

        while (continueRunning)
        {
            Console.WriteLine("\n\x1B[34mTo enter a new product - press\x1B[0m \x1B[31m'P'\x1B[0m \x1B[34m|| To search for a product - press \x1B[0m \x1B[32m'S'\x1B[0m \x1B[34m|| To quit - press\x1B[0m \x1B[33m'Q'\x1B[0m:");

            var keyInfo = Console.ReadKey(true);
            char userInputChar = char.ToUpper(keyInfo.KeyChar);

            switch (userInputChar)
            {
                case 'P':
                    AddProducts(productManager);

                    productManager.DisplayAllProducts();

                    break;
                case 'S':
                    PerformSearch(productManager);

                    break;
                case 'Q':
                    Console.WriteLine("\nDo you want to save the product list? \x1B[31m(y/n)\x1B[0m: ");

                    var saveKeyInfo = Console.ReadKey(true);
                    string? saveInput = char.ToLower(saveKeyInfo.KeyChar) == 'y' ? "y" : "n";

                    if (saveInput == "y")
                    {
                        productManager.SaveProductsToFile(filePath);
                    }

                    continueRunning = false;

                    Console.WriteLine("\nExiting application... Press any key to exit.");
                    Console.ReadKey();
                    break;

                default:

                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }

        }
    }

    static void PerformSearch(ProductManager productManager)
    {

        Console.Write($"\nEnter search term \x1B[33m(Product name or Category):\x1B[0m ");
        string? searchTerm = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            List<Product> foundProducts = productManager.SearchProducts(searchTerm);
            if (foundProducts.Any())
            {
                Console.WriteLine($"\nFound \x1B[31m{foundProducts.Count}\x1B[0m product(s) matching \x1B[32m'{searchTerm}'\x1B[0m:");
                productManager.DisplayProducts(foundProducts);
            }
            else
            {
                Console.WriteLine($"No products found matching \x1B[32m'{searchTerm}'\x1B[0m.");
            }
        }
    }

    static void AddProducts(ProductManager productManager)
    {
        while (true)
        {
            Console.WriteLine("\nEnter product details or write \u001b[31m'q'\u001b[0m: to quit:");
            Console.WriteLine();

            Console.Write("Product Name: ");

            string? productNameInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(productNameInput))
            {
                Console.WriteLine("Product name cannot be empty! Please enter a valid product name.");
                continue;
            }

            string productName = productNameInput.ToLower();

            if (productName == "q") break;

            Console.Write("Category: ");

            string? categoryInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(categoryInput))
            {
                Console.WriteLine("Category cannot be empty. Please enter a valid category.");
                continue;
            }

            string category = categoryInput.ToLower();

            Console.Write("Price: ");

            string? priceInput = Console.ReadLine()?.Trim();

            if (!decimal.TryParse(priceInput, out decimal price))
            {
                Console.WriteLine("Invalid price, please try again.");
                continue;
            }

            try
            {
                Product product = new Product(productName, category, price);

                productManager.AddProduct(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product: \x1B[31m{ex.Message}\x1B[0m");
            }

            Console.WriteLine("\nDo you want to add another product? \u001b[31m(Press any key to continue or 'q' to quit)\u001b[0m: ");

            var keyInfo = Console.ReadKey(true);

            if (char.ToLower(keyInfo.KeyChar) == 'q')
            {
                break;
            }
        }
    }

}
