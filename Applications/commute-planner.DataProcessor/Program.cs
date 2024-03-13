﻿using System.Text.Json;
using commute_planner.CommuteDatabase;
using commute_planner.CommuteDatabase.Models;
using commute_planner.DataProcessing;
using commute_planner.EventCollaboration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.AddRabbitMQ("messaging");
builder.AddNpgsqlDbContext<CommutePlannerDbContext>("commute_db",
    settings =>
        settings.ConnectionString = "Host=localhost;Database=commute_db");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
  Console.WriteLine("\nCancel keys pressed, exiting.");
  cts.Cancel();
};

// Add a console logger.
builder.Services.AddLogging(configure => configure.AddConsole());

// Add a hosted service
// This is the actual service this application is for
builder.Services.AddHostedService<DataProcessingService>();
builder.Services.AddHostedService<EventCollaborationService>();

var app = builder.Build();

// When running hosted services
await app.StartAsync(cts.Token);

// Simple inline model for defining the seed data constants
