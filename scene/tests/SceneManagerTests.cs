using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace scene.tests;

public class SceneManagerTests {
    private readonly Mock<IOptions<ScenesOptions>> ScenesOptionsMock = new();

    private readonly IServiceProvider Services = new ServiceCollection()
        .AddSingleton<MockScene>()
        .BuildServiceProvider();

    [Fact]
    public void NavigateTo_Throws_When_Scene_Not_Found() {
        // Arrange
        var scenesOptions = new ScenesOptions {
            InitialScene = "non-existent-scene",
            Scenes = new()
        };
        ScenesOptionsMock.Setup(x => x.Value).Returns(scenesOptions);

        var sceneManager = new SceneManager(ScenesOptionsMock.Object, Services);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => sceneManager.NavigateTo("some-path"));
    }

    [Fact]
    public void NavigateTo_Pushes_Scene_To_Stack() {
        // Arrange
        var scenesOptions = new ScenesOptions {
            Scenes = new() {
                {
                    "scene1",
                    new() {
                        Scene = typeof(MockScene),
                        Scenes = new() {
                            {
                                "scene2",
                                new() {
                                    Scene = typeof(MockScene)
                                }
                            }
                        }
                    }
                }
            },
            InitialScene = "scene1"
        };
        ScenesOptionsMock.Setup(x => x.Value).Returns(scenesOptions);

        var sceneManager = new SceneManager(ScenesOptionsMock.Object, Services);

        // Act
        sceneManager.NavigateTo("scene2");

        // Assert
        Assert.Equal("scene1/scene2", sceneManager.Location);
        Assert.Equal(2, sceneManager.SceneStack.Count);
        Assert.Equal("scene1", sceneManager.SceneStack.ToArray()[1].Path);
        Assert.Equal("scene1/scene2", sceneManager.SceneStack.ToArray()[0].Path);
    }

    [Fact]
    public void NavigateTo_Disposes_Scenes_Not_In_Path() {
        // Arrange
        var scenesOptions = new ScenesOptions {
            Scenes = new() {
                {
                    "scene1",
                    new() {
                        Scene = typeof(MockScene),
                        Scenes = new() {
                            {
                                "scene2",
                                new() {
                                    Scene = typeof(MockScene)
                                }
                            }
                        }
                    }
                }
            },
            InitialScene = "scene1/scene2"
        };
        ScenesOptionsMock.Setup(x => x.Value).Returns(scenesOptions);

        var sceneManager = new SceneManager(ScenesOptionsMock.Object, Services);

        // Act
        sceneManager.NavigateTo("/scene1");

        // Assert
        Assert.Equal("scene1", sceneManager.Location);
        Assert.Equal(1, sceneManager.SceneStack.Count);
        Assert.Equal("scene1", sceneManager.SceneStack.ToArray()[0].Path);
    }

    private class MockScene : IScene {
        public void Dispose() { }
    }
}