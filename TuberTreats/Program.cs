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
app.MapGet("/tuberorders", () => orders);


////Get endpoints

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

//Get topping by id
app.MapGet("/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);
    if (topping == null)
    {
        return Results.NotFound();
    }

    ToppingDTO toppingDTO = new ToppingDTO
    {
        Id = topping.Id,
        Name = topping.Name
    };

    return Results.Ok(toppingDTO);
});

//Get a customer by id, with their orders
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }

    List<TuberOrderDTO> customerOrders = orders
        .Where(o => o.CustomerId == id)
        .Select(o => new TuberOrderDTO
        {
            Id = o.Id,
            OrderPlacedOnDate = o.OrderPlacedOnDate,
            CustomerId = o.CustomerId,
            TuberDriverId = o.TuberDriverId,
            DeliveredOnDate = o.DeliveredOnDate,
            Toppings = tuberToppings
                .Where(tt => tt.TuberOrderId == o.Id)
                .Select(tt => new ToppingDTO
                {
                    Id = tt.ToppingId,
                    Name = toppings.First(t => t.Id == tt.ToppingId).Name
                })
                .ToList()
        })
        .ToList();

    CustomerWithOrdersDTO customerWithOrders = new CustomerWithOrdersDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        Orders = customerOrders
    };

    return Results.Ok(customerWithOrders);
});

//Get an employee by id with their deliveries
app.MapGet("/tuberdrivers/{id}", (int id) =>
{
    TuberDriver driver = tuberDrivers.FirstOrDefault(d => d.Id == id);
    if (driver == null)
    {
        return Results.NotFound();
    }

    List<TuberOrderDTO> driverDeliveries = orders
        .Where(o => o.TuberDriverId == id)
        .Select(o => new TuberOrderDTO
        {
            Id = o.Id,
            OrderPlacedOnDate = o.OrderPlacedOnDate,
            CustomerId = o.CustomerId,
            TuberDriverId = o.TuberDriverId,
            DeliveredOnDate = o.DeliveredOnDate,
            Toppings = tuberToppings
                .Where(tt => tt.TuberOrderId == o.Id)
                .Select(tt => new ToppingDTO
                {
                    Id = tt.ToppingId,
                    Name = toppings.First(t => t.Id == tt.ToppingId).Name
                })
                .ToList()
        })
        .ToList();

    DriverWithDeliveriesDTO driverWithDeliveries = new DriverWithDeliveriesDTO
    {
        Id = driver.Id,
        Name = driver.Name,
        Deliveries = driverDeliveries
    };

    return Results.Ok(driverWithDeliveries);
});



////Post endpoints

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

//Add a topping to a TuberOrder (return the new TuberTopping object to the client)(POST to /tuberorders/{id}/toppings)
app.MapPost("/tuberorders/{id}/toppings", (int id, int toppingId) =>
{
    TuberOrder order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound("Order not found");
    }

    Topping topping = toppings.FirstOrDefault(t => t.Id == toppingId);
    if (topping == null)
    {
        return Results.BadRequest("Topping not found");
    }

    TuberTopping newTuberTopping = new TuberTopping
    {
        Id = tuberToppings.Any() ? tuberToppings.Max(tt => tt.Id) + 1 : 1,
        TuberOrderId = id,
        ToppingId = toppingId
    };

    tuberToppings.Add(newTuberTopping);

    TuberToppingDTO tuberToppingDTO = new TuberToppingDTO
    {
        Id = newTuberTopping.Id,
        TuberOrderId = newTuberTopping.TuberOrderId,
        ToppingId = newTuberTopping.ToppingId
    };

    return Results.Ok(tuberToppingDTO);
});

//Add a Customer (POST to /customers)(return the new customer)
app.MapPost("/customers", (CustomerDTO newCustomerDTO) =>
{
    Customer newCustomer = new Customer
    {
        //Here I am using LinQ to get the max id from the customers list and add 1 to it to get the new id
        Id = customers.Any() ? customers.Max(c => c.Id) + 1 : 1,
        Name = newCustomerDTO.Name,
        Address = newCustomerDTO.Address
    };

    customers.Add(newCustomer);

    CustomerDTO createdCustomerDTO = new CustomerDTO
    {
        Id = newCustomer.Id,
        Name = newCustomer.Name,
        Address = newCustomer.Address
    };

    return Results.Ok(createdCustomerDTO);
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





////Delete endpoints

//Remove a topping from a TuberOrder (DELETE to /tuberorders/{id}/toppings/{toppingId})
app.MapDelete("/tuberorders/{orderId}/toppings/{toppingId}", (int orderId, int toppingId) =>
{
    TuberTopping tuberTopping = tuberToppings.FirstOrDefault(tt => tt.TuberOrderId == orderId && tt.ToppingId == toppingId);
    if (tuberTopping == null)
    {
        return Results.NotFound("Topping not found in the specified order");
    }

    tuberToppings.Remove(tuberTopping);

    TuberToppingDTO deletedToppingDTO = new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    };

    return Results.Ok(deletedToppingDTO);
});

//Delete a customer (DELETE to /customers/{id})
app.MapDelete("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }

    customers.Remove(customer);

    CustomerDTO deletedCustomerDTO = new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    };

    return Results.Ok(deletedCustomerDTO);
});

app.Run();
