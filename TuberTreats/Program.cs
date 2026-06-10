using TuberTreats.Models;
using TuberTreats.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


List<TuberDriver> tuberDrivers = new()
{
    new TuberDriver
    {
        Id = 1,
        Name = "Bob"
    },
    new TuberDriver
    {
        Id = 2,
        Name = "Sarah"
    },
    new TuberDriver
    {
        Id = 3,
        Name = "Mike"
    }
};

List<Customer> customers = new()
{
    new Customer
    {
        Id = 1,
        Name = "Shaquille O'Neal",
        Address = "123 Main St"
    },
    new Customer
    {
        Id = 2,
        Name = "Jane Smith",
        Address = "456 Oak Ave"
    },
    new Customer
    {
        Id = 3,
        Name = "John Davis",
        Address = "789 Pine Rd"
    },
    new Customer
    {
        Id = 4,
        Name = "Emily Johnson",
        Address = "101 Maple Dr"
    },
    new Customer
    {
        Id = 5,
        Name = "Mark Brown",
        Address = "202 Cedar Ln"
    }
};

List<Topping> toppings = new()
{
    new Topping
    {
        Id = 1,
        Name = "Cheese"
    },
    new Topping
    {
        Id = 2,
        Name = "Bacon"
    },
    new Topping
    {
        Id = 3,
        Name = "Chili"
    },
    new Topping
    {
        Id = 4,
        Name = "Sour Cream"
    },
    new Topping
    {
        Id = 5,
        Name = "Butter"
    }
};

List<TuberOrder> tuberOrders = new()
{
    new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate = DateTime.Now.AddHours(-3),
        CustomerId = 1,
        TuberDriverId = 1,
        DeliveredOnDate = DateTime.Now.AddHours(-1),
        Toppings = new List<TuberTopping>
        {
            new TuberTopping
            {
                Id = 1,
                TuberOrderId = 1,
                ToppingId = 1
            },
            new TuberTopping
            {
                Id = 2,
                TuberOrderId = 1,
                ToppingId = 2
            }
        }
    },

    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate = DateTime.Now.AddHours(-2),
        CustomerId = 3,
        TuberDriverId = null,
        DeliveredOnDate = null,
        Toppings = new List<TuberTopping>
        {
            new TuberTopping
            {
                Id = 3,
                TuberOrderId = 2,
                ToppingId = 3
            },
            new TuberTopping
            {
                Id = 4,
                TuberOrderId = 2,
                ToppingId = 4
            }
        }
    },

    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate = DateTime.Now.AddHours(-1),
        CustomerId = 5,
        TuberDriverId = 2,
        DeliveredOnDate = null,
        Toppings = new List<TuberTopping>()
    }
};

//add endpoints here
app.MapGet("/tuberorders", () =>
{
    return Results.Ok(tuberOrders);
});

app.MapGet("/tuberorders/{id}", (int id) =>
{
    TuberOrder order = tuberOrders
        .FirstOrDefault(order => order.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    Customer customer = customers
        .FirstOrDefault(customer => customer.Id == order.CustomerId);

    TuberDriver driver = tuberDrivers
        .FirstOrDefault(driver => driver.Id == order.TuberDriverId);

    List<ToppingDTO> toppingDTOs = order.Toppings
        .Select(tuberTopping =>
        {
            Topping topping = toppings
                .FirstOrDefault(toppings => toppings.Id == tuberTopping.ToppingId);

            return new ToppingDTO
            {
                Id = topping.Id,
                Name = topping.Name
            };
        })
        .ToList();

    TuberOrderDTO response = new TuberOrderDTO
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        DeliveredOnDate = order.DeliveredOnDate,

        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },

        Driver = driver == null
            ? null
            : new TuberDriverDTO
            {
                Id = driver.Id,
                Name = driver.Name
            },

        Toppings = toppingDTOs
    };

    return Results.Ok(response);
});

app.MapPost("/tuberorders", (TuberOrder newOrder) => {
    newOrder.Id = tuberOrders.Max(order => order.Id) + 1;
    newOrder.OrderPlacedOnDate = DateTime.Now;
    tuberOrders.Add(newOrder);
    return Results.Created($"/tuberorders/{newOrder.Id}", newOrder);
});

app.MapPut("/tuberorders/{id}", (int id, int driverId) =>
{
    TuberOrder order = tuberOrders
    .FirstOrDefault(order => order.Id == id);
    
    if (order == null)
    {
        return Results.NotFound();
    }
    TuberDriver driver = tuberDrivers
        .FirstOrDefault(driver => driver.Id == driverId);

    if (driver == null)
    {
        return Results.BadRequest();
    }

    order.TuberDriverId = driverId;

    return Results.NoContent();
});

app.MapPost("/tuberorders/{id}/complete", (int id) =>
{
    TuberOrder order = tuberOrders
        .FirstOrDefault(order => order.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    order.DeliveredOnDate = DateTime.Now;

    return Results.Ok(order);
});

app.MapGet("/toppings", () =>
{
    return Results.Ok(toppings);
});

app.MapGet("/toppings/{id}", (int id) =>
{
    Topping topping = toppings
        .FirstOrDefault(topping => topping.Id == id);
    
    if (topping == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(topping);
});

app.MapGet("/tubertoppings", () =>
{
    return Results.Ok(tuberOrders
    .SelectMany(order => order.Toppings)
    .ToList());
});


app.MapPost("/tubertoppings", (TuberTopping newTopping) =>
{
    TuberOrder order = tuberOrders
        .FirstOrDefault(order => order.Id == newTopping.TuberOrderId);

    if (order == null)
    {
        return Results.NotFound();
    }

    newTopping.Id = tuberOrders
        .SelectMany(order => order.Toppings)
        .Max(topping => topping.Id) + 1;

    order.Toppings.Add(newTopping);

    return Results.Created(
        $"/tubertoppings/{newTopping.Id}",
        newTopping
    );
});

app.MapDelete("/tubertoppings/{id}", (int id) =>
{
   TuberTopping tuberTopping = tuberOrders
        .SelectMany(order => order.Toppings)
        .FirstOrDefault(topping => topping.Id == id); 

    if (tuberTopping == null)
    {
        return Results.NotFound();
    }

    TuberOrder order = tuberOrders
        .FirstOrDefault(order => order.Toppings.Any(topping => topping.Id == id));
    
    order.Toppings.Remove(tuberTopping);

    return Results.NoContent();
});

app.MapGet("/customers", () =>
{
    return Results.Ok(customers);
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers 
        .FirstOrDefault(customer => customer.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    List<TuberOrderDTO> customerOrders = tuberOrders
        .Where(order => order.CustomerId == id)
        .Select(order => new TuberOrderDTO
        {
            Id = order.Id,
            OrderPlacedOnDate = order.OrderPlacedOnDate,
            DeliveredOnDate = order.DeliveredOnDate,

        })
        .ToList();

    CustomerDTO customerDTO = new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        TuberOrders = customerOrders
    };

    return Results.Ok(customerDTO);
});

app.MapPost("/customers", (Customer customer) =>
{
    customer.Id = customers.Max(customer => customer.Id) + 1;
    customers.Add(customer);

    return Results.Created(
        $"/customers/{customer.Id}",
        customer
    );
});

app.MapDelete("/customers/{id}", (int id) =>
{
    Customer customer = customers
        .FirstOrDefault(customer => customer.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    customers.Remove(customer);

    return Results.NoContent();
});

app.MapGet("/tuberdrivers", () =>
{
    return Results.Ok(tuberDrivers);
});

app.MapGet("/tuberdrivers/{id}", (int id) =>
{
    TuberDriver driver = tuberDrivers
        .FirstOrDefault(driver => driver.Id == id);

    if (driver == null)
    {
        return Results.NotFound();
    }

    List<TuberOrderDTO> deliveries = tuberOrders
        .Where(order => order.TuberDriverId == id)
        .Select(order => new TuberOrderDTO
        {
          Id = order.Id,
          OrderPlacedOnDate = order.OrderPlacedOnDate,
          DeliveredOnDate = order.DeliveredOnDate  
        })
        .ToList();


    TuberDriverDTO driverDTO = new TuberDriverDTO
    {
        Id = driver.Id,
        Name = driver.Name,
        TuberDeliveries = deliveries
    };

    return Results.Ok(driverDTO);
});

app.Run();
//don't touch or move this!
public partial class Program { }