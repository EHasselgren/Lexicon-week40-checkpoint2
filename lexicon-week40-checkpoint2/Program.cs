using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public override string ToString()
    {
        return $"Product name: \x1B[31m{ProductName}\x1B[0m Category: \x1B[32m{Category}\x1B[0m Price: \x1B[33m{Price:C}\x1B[0m";
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

            Console.WriteLine("Product added successfully!");
        }
        else
        {
            Console.WriteLine($"A product with the name: \x1B[31m'{product.ProductName}'\x1B[0m  and category: \x1B[32m'{product.Category}'\x1B[0m already exists.");
        }
    }

    public void DisplayProducts(string? highlightSearch = null)
    {
        if (!products.Any())
        {
            Console.WriteLine("No products added.");
            return;
        }

        var sortedProducts = products.OrderBy(p => p.Price).ToList();
        decimal totalPrice = products.Sum(p => p.Price);

        Console.WriteLine("\nProducts List  \x1B[33m(Sorted by Price) \x1B[0m:");

        foreach (var product in sortedProducts)
        {
            bool matchesHighlight = !string.IsNullOrEmpty(highlightSearch) &&
                                   (product.ProductName.Contains(highlightSearch, StringComparison.OrdinalIgnoreCase) ||
                                    product.Category.Contains(highlightSearch, StringComparison.OrdinalIgnoreCase));

            string formattedProduct = $"Product name: \x1B[31m{product.ProductName}\x1B[0m Category: \x1B[32m{product.Category}\x1B[0m Price: \x1B[33m{product.Price:C}\x1B[0m";

            if (matchesHighlight)
            {
                Console.WriteLine($"\x1B[93m{formattedProduct}\x1B[0m");
            }
            else
            {
                Console.WriteLine(formattedProduct);
            }
        }

        Console.WriteLine($"\nTotal Price of all products: \x1B[93m{totalPrice:C}\x1B[0m");
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

                Console.WriteLine("Products successfully loaded from file.");
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
    static void PerformSearch(ProductManager productManager)
    {
        Console.WriteLine("\nWould you like to search for a product? \x1B[31m(y/n)\x1B[0m:");

        string? searchInput = Console.ReadLine()?.ToLower();

        if (searchInput == "y")
        {
            Console.Write("Enter search term \x1B[33m(Product name or Category):\x1B[0m");

            string? searchTerm = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                List<Product> foundProducts = productManager.SearchProducts(searchTerm);

                if (foundProducts.Any())
                {
                    Console.WriteLine($"\nFound  \x1B[31m{foundProducts.Count}\x1B[0m product(s) matching \x1B[32m'{searchTerm}'\x1B[0m:");

                    productManager.DisplayProducts(searchTerm);
                }
                else
                {
                    Console.WriteLine($"No products found matching \x1B[32m'{searchTerm}'\x1B[0m.");
                }
            }
        }
    }


    static void Main(string[] args)
    {
        var productManager = new ProductManager();

        string filePath = "products.json";

        productManager.LoadProductsFromFile(filePath);


        bool continueAdding = true;
        while (continueAdding)
        {
            AddProducts(productManager);

            productManager.DisplayProducts();

            Console.WriteLine("\nDo you want to add more products? \x1B[31m(y/n)\x1B[0m: ");

            string? continueInput = Console.ReadLine()?.ToLower();

            continueAdding = continueInput == "y";
        }

        PerformSearch(productManager);

        Console.WriteLine("\nDo you want to save the product list? \x1B[31m(y/n)\x1B[0m: ");

        string? saveInput = Console.ReadLine()?.ToLower();

        if (saveInput == "y")
        {
            productManager.SaveProductsToFile(filePath);
        }
    }

    static void AddProducts(ProductManager productManager)
    {
        while (true)
        {
            Console.WriteLine("\nEnter product details or write \u001b[31m'q'\u001b[0m: to quit:");
            Console.Write("Product Name: ");

            string? productNameInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(productNameInput))
            {
                Console.WriteLine("Product name cannot be empty. Please enter a valid product name.");
                continue;
            }

            string productName = productNameInput.ToLower();

            if (productName == "q") break;

            Console.Write("Category: ");

            string? categoryInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(categoryInput))
            {
                Console.WriteLine("Category cannot be empty. Please enter a valid category.");
                continue;
            }

            string category = categoryInput.ToLower();

            Console.Write("Price: ");

            string? priceInput = Console.ReadLine();

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
                Console.WriteLine($"Error adding product:\x1B[31m {ex.Message}\u001b[0m");
            }
        }
    }

}
