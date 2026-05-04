using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using SubscriptionWeb.Controllers;
using SubscriptionWeb.Models;
using SubscriptionWeb.Repositories;
using System.Security.Claims;
using Xunit;

namespace SubscriptionWeb.Tests
{
    public class PlanControllerTests
    {
        private readonly Mock<IPlanRepository> _planRepoMock;
        private readonly Mock<IServiceRepository> _serviceRepoMock;
        private readonly PlanController _controller;
        private const int TestUserId = 1;

        public PlanControllerTests()
        {
            _planRepoMock = new Mock<IPlanRepository>();
            _serviceRepoMock = new Mock<IServiceRepository>();

            _controller = new PlanController(_planRepoMock.Object, _serviceRepoMock.Object);

            // Имитируем авторизованного пользователя
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("UserId", TestUserId.ToString()),
                new Claim(ClaimTypes.Name, "TestUser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        #region Index Tests
        [Fact]
        public async Task Index_ReturnsViewWithPlans_WhenPlansExist()
        {
            var mockPlans = new List<Plan> { new Plan { PlanId = 1, UserId = TestUserId } };
            _planRepoMock.Setup(repo => repo.GetAllAsync(TestUserId)).ReturnsAsync(mockPlans);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Plan>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }
        #endregion

        #region Create Tests
        [Fact]
        public async Task Create_Get_ReturnsViewWithServicesList()
        {
            var mockServices = new List<Service> { new Service { ServiceId = 1, Name = "Spotify" } };
            _serviceRepoMock.Setup(repo => repo.GetAllAsync(TestUserId)).ReturnsAsync(mockServices);

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Services);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
        {
            var newPlan = new Plan { PlanName = "Premium", Price = 100, BillingDays = 30 };

            var result = await _controller.Create(newPlan);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _planRepoMock.Verify(r => r.AddAsync(It.Is<Plan>(p => p.UserId == TestUserId)), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("PlanName", "Required");
            var invalidPlan = new Plan { Price = 100 };
            _serviceRepoMock.Setup(repo => repo.GetAllAsync(TestUserId)).ReturnsAsync(new List<Service>());

            var result = await _controller.Create(invalidPlan);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidPlan, viewResult.Model);
            _planRepoMock.Verify(r => r.AddAsync(It.IsAny<Plan>()), Times.Never);
        }
        #endregion

        #region Edit Tests
        [Fact]
        public async Task Edit_Get_ReturnsView_WhenPlanExists()
        {
            var plan = new Plan { PlanId = 1, UserId = TestUserId, ServiceId = 1 };
            _planRepoMock.Setup(repo => repo.GetByIdAsync(1, TestUserId)).ReturnsAsync(plan);
            _serviceRepoMock.Setup(repo => repo.GetAllAsync(TestUserId)).ReturnsAsync(new List<Service>());

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(plan, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenPlanDoesNotExist()
        {
            _planRepoMock.Setup(repo => repo.GetByIdAsync(99, TestUserId)).ReturnsAsync((Plan)null);

            var result = await _controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_RedirectsToIndex_WhenUpdateIsSuccessful()
        {
            var planToUpdate = new Plan { PlanId = 1, PlanName = "Updated" };

            var result = await _controller.Edit(planToUpdate);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _planRepoMock.Verify(r => r.UpdateAsync(It.Is<Plan>(p => p.UserId == TestUserId)), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Price", "Invalid");
            var plan = new Plan { PlanId = 1 };
            _serviceRepoMock.Setup(repo => repo.GetAllAsync(TestUserId)).ReturnsAsync(new List<Service>());

            var result = await _controller.Edit(plan);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(plan, viewResult.Model);
            _planRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Plan>()), Times.Never);
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task Delete_Post_RedirectsToIndex_AfterDeletion()
        {
            var result = await _controller.Delete(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _planRepoMock.Verify(r => r.DeleteAsync(1, TestUserId), Times.Once);
        }
        #endregion
    }
}