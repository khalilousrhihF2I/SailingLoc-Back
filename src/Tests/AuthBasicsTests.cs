using Xunit;
using FluentAssertions;
namespace Tests;
public class AuthBasicsTests {
  [Fact] public void TrueIsTrue() => true.Should().BeTrue();
}
