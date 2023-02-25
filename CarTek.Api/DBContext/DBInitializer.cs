using CarTek.Api.Model;
using System.Diagnostics;

namespace CarTek.Api.DBContext
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Look for any students.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            SeedUsers(context);

            SeedDrivers(context);

            SeedTrailers(context);

            SeedCars(context);
        }

        private static void SeedUsers(ApplicationDbContext context)
        {
            var students = new User[]
            {
                new User{FirstName="Тимофей", LastName="Щербаков", MiddleName="Валерьевич", Email="shcherbakov.t@gmail.com", IsAdmin=true, Phone="79110109825", Login="tshcherb"},
                new User{FirstName="Полина", LastName="Щербакова", MiddleName="Вячеславовна", Email="shcherbakov.t@gmail.com", IsAdmin=true, Phone="79110109825", Login="polina"},
                new User{FirstName="Попик", LastName="Попугов", MiddleName="Попугаевич", Email="shcherbakov.t@gmail.com", IsAdmin=true, Phone="79110109825", Login="popugai"},
                new User{FirstName="Фил", LastName="Щербаков", MiddleName="Валерьевич", Email="shcherbakov.t@gmail.com", IsAdmin=true, Phone="79110109825", Login="tshcherb"},
            };

            context.Users.AddRange(students);

            context.SaveChanges();
        }

        private static void SeedDrivers(ApplicationDbContext context)
        {
            var drivers = new Driver[]
            {
                new Driver{FirstName="Тимофей1", LastName="Щербаков", MiddleName="Валерьевич", Email="1shcherbakov.t@gmail.com", Phone="79110109825", Password="asdas"},
                new Driver{FirstName="Тимофей2", LastName="Щербаков", MiddleName="Валерьевич", Email="2shcherbakov.t@gmail.com", Phone="79110109826", Password="test5"},
                new Driver{FirstName="Тимофей3", LastName="Щербаков", MiddleName="Валерьевич", Email="3shcherbakov.t@gmail.com", Phone="79110109827", Password = "test4"},
                new Driver{FirstName="Тимофей4", LastName="Щербаков", MiddleName="Валерьевич", Email="4shcherbakov.t@gmail.com", Phone="79110109828", Password = "test1"}
            };

            context.Drivers.AddRange(drivers);
            context.SaveChanges();
        }

        private static void SeedTrailers(ApplicationDbContext context)
        {
            var trailers = new Trailer[]
            {
                new Trailer{Brand = "Traier1", Model="X4", Plate="H257MC198", State="Base"},
                new Trailer{Brand = "Traier2", Model = "X5", Plate = "H357MC198", State = "Base"},
                new Trailer{Brand = "Traier3", Model = "X6", Plate = "H457MC198", State = "Base"}
            };

            context.Trailers.AddRange(trailers);

            context.SaveChanges();
        }        
                
        private static void SeedCars(ApplicationDbContext context)
        {
            var driver1 = context.Drivers.FirstOrDefault(t => t.Id == 1);
            var driver2 = context.Drivers.FirstOrDefault(t => t.Id == 2);
            var driver3 = context.Drivers.FirstOrDefault(t => t.Id == 3);

            var trailer1 = context.Trailers.FirstOrDefault(t => t.Id == 1);
            var trailer2 = context.Trailers.FirstOrDefault(t => t.Id == 2);
            var trailer3 = context.Trailers.FirstOrDefault(t => t.Id == 3);

            var cars = new Car[]
            {
                new Car{Brand = "BMW", Model="X1", Plate="H257MC198", State = TransportState.Base, Driver = driver1, Trailer = trailer1},
                new Car{Brand = "BMW", Model = "X2", Plate = "H357MC198", State = TransportState.Line, Driver = driver2, Trailer = trailer2},
                new Car{Brand = "BMW", Model = "X3", Plate = "H457MC198", State = TransportState.Base, Driver = driver3, Trailer = trailer3}
            };

            context.Cars.AddRange(cars);

            context.SaveChanges();
        }
    }
}
