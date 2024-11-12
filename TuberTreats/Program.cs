using TuberTreats.Models;
using TuberTreats.Models.DTOs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Alice Smith", Address = "123 Maple Street, Springfield" },
    new Customer { Id = 2, Name = "Bob Johnson", Address = "456 Oak Avenue, Metropolis" },
    new Customer { Id = 3, Name = "Catherine Brown", Address = "789 Pine Road, Gotham" },
    new Customer { Id = 4, Name = "David Wilson", Address = "101 Birch Boulevard, Star City" },
    new Customer { Id = 5, Name = "Evelyn Turner", Address = "202 Cedar Lane, Central City" }
};


List<TuberDriver> tuberDrivers = new List<TuberDriver>
{
    new TuberDriver { Id = 1, Name = "John Doe" },
    new TuberDriver { Id = 2, Name = "Jane Smith" },
    new TuberDriver { Id = 3, Name = "Michael Johnson" }
};

List<Topping> toppings = new List<Topping>
{
    new Topping { Id = 1, Name = "Pepperoni" },
    new Topping { Id = 2, Name = "Mushrooms" },
    new Topping { Id = 3, Name = "Onions" },
    new Topping { Id = 4, Name = "Green Peppers" },
    new Topping { Id = 5, Name = "Sausage" }
};

List<TuberOrder> orders = new List<TuberOrder>
{
    new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate = new DateTime(2024, 11, 1, 14, 30, 0),
        CustomerId = 1,
        TuberDriverId = null,
        DeliveredOnDate = null
    },
    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate = new DateTime(2024, 11, 2, 10, 0, 0),
        CustomerId = 2,
        TuberDriverId = 2,
        DeliveredOnDate = new DateTime(2024, 11, 2, 10, 45, 0)
    },
    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate = new DateTime(2024, 11, 3, 16, 15, 0),
        CustomerId = 3,
        TuberDriverId = 3,
        DeliveredOnDate = new DateTime(2024, 11, 3, 17, 0, 0)
    }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping { Id = 1, TuberOrderId = 1, ToppingId = 1 },
    new TuberTopping { Id = 2, TuberOrderId = 1, ToppingId = 3 },
    new TuberTopping { Id = 3, TuberOrderId = 2, ToppingId = 2 },
    new TuberTopping { Id = 4, TuberOrderId = 2, ToppingId = 4 },
    new TuberTopping { Id = 5, TuberOrderId = 3, ToppingId = 1 },
    new TuberTopping { Id = 6, TuberOrderId = 3, ToppingId = 5 }
};


//Get All endpoints
app.MapGet("/tuberdrivers", () => tuberDrivers);
app.MapGet("/customers", () => customers);
app.MapGet("/tubertoppings", () => tuberToppings);
app.MapGet("/toppings", () => toppings);



////Get endpoints

//Get all orders
app.MapGet("/tuberorders", () => orders);

//Get an order by id (must include customer data as well as driver and toppings data, if applicable).
app.MapGet("/tuberorders/{id}", (int id) =>
{
    TuberOrder order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound();
    }

    Customer customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);

    TuberDriver driver = null;
    if (order.TuberDriverId.HasValue)
    {
        driver = tuberDrivers.FirstOrDefault(d => d.Id == order.TuberDriverId.Value);
    }

    List<Topping> orderToppings = new List<Topping>();
    foreach (TuberTopping tuberTopping in tuberToppings)
    {
        if (tuberTopping.TuberOrderId == order.Id)
        {
            Topping topping = toppings.FirstOrDefault(t => t.Id == tuberTopping.ToppingId);
            if (topping != null)
            {
                orderToppings.Add(topping);
            }
        }
    }

    OrderDetailsDTO orderDetails = new OrderDetailsDTO
    {
        Order = order,
        Customer = customer,
        Driver = driver,
        Toppings = orderToppings
    };

    return Results.Ok(orderDetails);
});







//Post endpoints

//Submit a new order (the API should add an OrderPlacedOnDate). Return the new order so the client can see the new Id.
app.MapPost("/tuberorders", (TuberOrderDTO newOrder) =>
{
    TuberOrder tuberOrder = new TuberOrder
    {
        Id = orders.Any() ? orders.Max(o => o.Id) + 1 : 1,
        OrderPlacedOnDate = DateTime.Now,
        CustomerId = newOrder.CustomerId,
        TuberDriverId = newOrder.TuberDriverId,
        DeliveredOnDate = newOrder.DeliveredOnDate
    };

    List<Topping> toppingsForOrder = new List<Topping>();
    foreach (ToppingDTO toppingDTO in newOrder.Toppings)
    {
        Topping topping = toppings.FirstOrDefault(t => t.Id == toppingDTO.Id);
        if (topping != null)
        {
            toppingsForOrder.Add(topping);
            tuberToppings.Add(new TuberTopping
            {
                Id = tuberToppings.Any() ? tuberToppings.Max(tt => tt.Id) + 1 : 1,
                TuberOrderId = tuberOrder.Id,
                ToppingId = topping.Id
            });
        }
    }

    tuberOrder.Toppings = toppingsForOrder;
    orders.Add(tuberOrder);

    TuberOrderDTO createdOrder = new TuberOrderDTO
    {
        Id = tuberOrder.Id,
        OrderPlacedOnDate = tuberOrder.OrderPlacedOnDate,
        CustomerId = tuberOrder.CustomerId,
        TuberDriverId = tuberOrder.TuberDriverId,
        DeliveredOnDate = tuberOrder.DeliveredOnDate,
        Toppings = newOrder.Toppings
    };

    return Results.Ok(createdOrder);
});

//Complete an order (POST to /tuberorders/{id}/complete)
app.MapPost("/tuberorders/{id}/complete", (int id) =>
{
    TuberOrder order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound();
    }

    order.DeliveredOnDate = DateTime.Now;

    OrderCompletionDTO completionDetails = new OrderCompletionDTO
    {
        DeliveredOnDate = order.DeliveredOnDate.Value
    };

    return Results.Ok(completionDetails);
});







////Put endpoints

//Assign a driver to an order (PUT to /tuberorders/{id})
app.MapPut("/tuberorders/{id}", (int id, OrderDriverAssignmentDTO driverAssignment) =>
{
    TuberOrder order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound();
    }

    TuberDriver driver = tuberDrivers.FirstOrDefault(d => d.Id == driverAssignment.TuberDriverId);
    if (driver == null)
    {
        return Results.BadRequest("Driver not found");
    }

    order.TuberDriverId = driverAssignment.TuberDriverId;

    OrderDriverAssignmentDTO updatedAssignment = new OrderDriverAssignmentDTO
    {
        OrderId = order.Id,
        TuberDriverId = order.TuberDriverId
    };

    return Results.Ok(updatedAssignment);
});


app.Run();
