// See https://aka.ms/new-console-template for more information
using ClassDemoTransaction.model;
using ClassDemoTransaction.services;
using System.Data.Common;

MyDBTransaction db = new MyDBTransaction();

Konto konto = db.GetById(1000); // peters konto
Console.WriteLine(konto);

decimal nytBeloeb = db.InsertAmount(1000, 3000);
Console.WriteLine(nytBeloeb);


Console.WriteLine(" === FØR ===");
Console.WriteLine(db.GetById(1000));
Console.WriteLine(db.GetById(1100));


db.MoveAmountUsingTransaction(1000, 1100, 10000);

Console.WriteLine(" === EFTER ===");
Console.WriteLine(db.GetById(1000));
Console.WriteLine(db.GetById(1100));

