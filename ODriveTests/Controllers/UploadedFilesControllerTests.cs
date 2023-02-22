using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ODrive.Models;

namespace ODrive.Controllers.Tests
{
    [TestClass()]
    public class UploadedFilesControllerTests
    {
        // Future work to create integration tests which
        // will create mock HTTP client to test the controller:
        // https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1.

        UploadedFilesController controller = new UploadedFilesController(new Models.ODriveContext(
            new DbContextOptionsBuilder<Models.ODriveContext>()
                .UseInMemoryDatabase(databaseName: "ODrive")
                .Options));

        [TestMethod()]
        public void GetHealthTest()
        {
            var result = controller.GetHealth();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));
        }

        [TestMethod()]
        public void PostUploadedFileTest()
        {
            var stream = new FileStream("Artifacts/sample.mp4", FileMode.Open);
            var formFile = new FormFile(stream, 0, stream.Length, "sample", "sample.mp4");

            var result = controller.PostUploadedFile(formFile);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(ActionResult<UploadedFile>));

        }
    }
}