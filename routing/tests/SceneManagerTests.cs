using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace routing.tests;

public class SceneManagerTests {
    private readonly Mock<IOptions<RouteOptions>> ScenesOptionsMock = new();

    private readonly IServiceProvider Services = new ServiceCollection()
        .AddSingleton<MockRoute>()
        .BuildServiceProvider();

    [Fact]
    public void NavigateTo_Throws_When_Scene_Not_Found() {
        // Arrange
        var scenesOptions = new RouteOptions {
            InitialRoute = "non-existent-scene",
            Routes = new()
        };
        ScenesOptionsMock.Setup(x => x.Value).Returns(scenesOptions);

        var sceneManager = new RouteManager(ScenesOptionsMock.Object, Services);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sceneManager.NavigateTo("some-path"));
    }

    [Fact]
    public void NavigateTo_Pushes_Scene_To_Stack() {
        // Arrange
        var scenesOptions = new RouteOptions {
            Routes = new() {
                {
                    "scene1",
                    new(
                        typeof(MockRoute),
                        new() {
                            {
                                "scene2",
                                new(typeof(MockRoute))
                            }
                        }
                    )
                }
            },
            InitialRoute = "scene1"
        };
        ScenesOptionsMock.Setup(x => x.Value).Returns(scenesOptions);

        var sceneManager = new RouteManager(ScenesOptionsMock.Object, Services);

        // Act
        sceneManager.NavigateTo("scene2");

        // Assert
        Assert.Equal("scene1/scene2", sceneManager.Location);
        Assert.Equal(2, sceneManager.RouteStack.Count);
        Assert.Equal("scene1", sceneManager.RouteStack.ToArray()[1].Path);
        Assert.Equal("scene1/scene2", sceneManager.RouteStack.ToArray()[0].Path);
    }

    [Fact]
    public void NavigateTo_Disposes_Scenes_Not_In_Path() {
        // Arrange
        var scenesOptions = new RouteOptions {
            Routes = new() {
                {
                    "scene1",
                    new(typeof(MockRoute), new() {
                        {
                            "scene2",
                            new(typeof(MockRoute))
                        }
                    })
                }
            },
            InitialRoute = "scene1/scene2"
        };
        ScenesOptionsMock.Setup(x => x.Value).Returns(scenesOptions);

        var sceneManager = new RouteManager(ScenesOptionsMock.Object, Services);

        // Act
        sceneManager.NavigateTo("/scene1");

        // Assert
        Assert.Equal("scene1", sceneManager.Location);
        Assert.Equal(1, sceneManager.RouteStack.Count);
        Assert.Equal("scene1", sceneManager.RouteStack.ToArray()[0].Path);
    }

    private class MockRoute : IRoute {
        public void Dispose() { }
    }
}