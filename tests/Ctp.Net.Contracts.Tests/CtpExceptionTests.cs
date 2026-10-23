namespace Ctp.Net.Contracts.Tests;

/// <summary>
/// 异常契约：<see cref="CtpBusinessException.ErrorId"/> 的精度是 <c>ICtpSession</c> 接缝的既定约定
/// （§16.1：把 Message 包装文本当错误传会破坏 native 帧），所以码与消息必须原样穿过，不能被格式化吃掉。
/// </summary>
public class CtpExceptionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3)]        // CTP 登录失败
    [InlineData(31)]       // 不在交易时段
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void ErrorId原样保留(int errorId)
    {
        var ex = new CtpBusinessException(errorId, "x");
        Assert.Equal(errorId, ex.ErrorId);
        Assert.Equal("x", ex.ErrorMsg);
    }

    [Fact]
    public void 消息里同时带码与原文()
    {
        var ex = new CtpBusinessException(3, "CTP:不合法的登录");
        Assert.Equal("CTP 业务错误 [3]: CTP:不合法的登录", ex.Message);
        Assert.Contains("不合法的登录", ex.Message);
    }

    [Fact]
    public void 业务异常是契约异常的子类()
        => Assert.IsAssignableFrom<CtpException>(new CtpBusinessException(1, "e"));

    [Fact]
    public void 契约异常可直接抛可捕获()
    {
        // 显式声明成 Action：throw 表达式会同时匹配 Func<Task> 重载，那个重载是 async 测试的入口（已过时）。
        Action throwIt = () => throw new CtpException("boom");
        var caught = Assert.Throws<CtpException>(throwIt);
        Assert.Equal("boom", caught.Message);
    }

    [Fact]
    public void 空错误消息不炸()
    {
        var ex = new CtpBusinessException(7, "");
        Assert.Equal(7, ex.ErrorId);
        Assert.Equal("", ex.ErrorMsg);
    }
}
