# Velox.DB
Portable lightweight .NET ORM for mobile, desktop and servers

Velox.DB is a .NET ORM that can be used on any platform supported by .NET, Mono and Xamarin.

Features:
- Works on any .NET 4.5+ platform, including Xamarin and Mono
- Lightweight and very fast
- Full LINQ expression support, including complex expressions involving relations
- Seamless support for relations (many-to-one and one-to-many)
- Uses POCO classes (no base class required)
- Works with any storage backend, including non-relational databases and flat files (Json/Xml)

### Velox.DB in action

Defining your object classes:

```csharp
public class Customer
{
   [Column.PrimaryKey(AutoIncrement = true)]
   public int CustomerID;
   [Column.NotNull]
   public string Name;
   
   public IDataSet<Order> Orders;
}

public class Order
{
   [Column.PrimaryKey(AutoIncrement = true)]
   public int OrderID;
   public int CustomerID;
   public DateTime Date;
   
   public Customer Customer;
}
```

(note that you can use public fields or properties)

Connect to a storage backend:

```csharp
var dbContext = new Vx.Context(new SqliteDataProvider("mydb.sqlite"));

// Create tables

dbContext.CreateTable<Customer>();
dbContext.CreateTable<Order>();
```

Create a record in the database:

```csharp
Customer customer = new Customer { Name = "John Doe" };

dbContext.DataSet<Customer>.Save(customer);

Console.WriteLine("Created customer ID {0}", customer.CustomerID");
```

Ok, so having to use dbContext.DataSet<Customer>() may be a little too much typing...

```csharp
var DB = new {
            Customers = dbContext.DataSet<Customer>(),
            Orders = dbContext.DataSet<Order>()
         };
// This works because DataSets are immutable and lightweight objects that are bound to the data store

// now it's a little easier (but there are other ways too)

DB.Customers.Save(customer);
```


Add a related record:

```csharp
Order order = new Order { Date = DateTime.Today, Customer = customer };

DB.Orders.Save(order);
```

Read relations:

```csharp
Customer customer = DB.Customers.Read(1);

foreach (Order order in customer.Orders)
   Console.WriteLine("Order ID = {0}" , order.OrderID);

// The many-to-one relation was automatically (lazy) populated because it was declared as DataSet<T>
// You can declare relations using any collection type, like IEnumerable<Order> or Order[] but you
// would need to tell Velox.DB to read the relation by calling Vx.LoadRelations(...)
```

LINQ queries:

```csharp

var customers = from customer in DB.Customers 
                   where customer.Orders.Any() 
                   order by customer.Name 
                   select customer;
                   
// This query is automatically translated to correct SQL if the data provider supports it:
//
// select * from Customer c where exists (select * from Order o where o.CustomerID=c.CustomerID) order by c.Name
```

Complex cross-relation LINQ queries:

LINQ queries:

```csharp

// Get all orders belonging to customers whos name start with "A"
var orders = from order in DB.Orders 
                   where order.Customer.Name.StartsWith("A")
                   select order;
                   
// Translates to:
//
// select * from Order o inner join Customer c on c.CustomerID=o.CustomerID where c.Name like 'A%'
```
