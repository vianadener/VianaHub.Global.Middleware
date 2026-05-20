// <copyright file="MethodExtensionsShortGuidTests.cs" company="Fidelidade">
// Copyright (c) Fidelidade. All rights reserved.
// </copyright>
using EBL.FIG.Common.Middleware.Lib.Extensions;

namespace EBL.FIG.Common.Middleware.Tests.Extensions;

/// <summary>
/// Testes que validam o comportamento do método <see cref="MethodExtensions.OrderGuid"/>.
///
/// Um COMB Guid gerado via RT.Comb (Provider.Sql) é:
///   • Um Guid válido e não-vazio.
///   • Sequencial: dois Guids gerados em sequência têm o segundo maior que o primeiro
///     quando comparados como strings SQL-Server-index-friendly (comparação byte-a-byte).
///   • Determinístico quanto ao formato: sempre 32 dígitos hexadecimais com hifens (formato "D").
///   • Único: dois Guids gerados consecutivamente são diferentes.
/// </summary>
public class MethodExtensionsShortGuidTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Validade do Guid
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "OrderGuid deve retornar um Guid diferente de Empty")]
    [Trait("Extensions", "")]
    public void ShortGuid_Chamado_DeveRetornarGuidNaoVazio()
    {
        // Act
        var result = MethodExtensions.OrderGuid();

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact(DisplayName = "OrderGuid deve retornar um Guid válido e parseável")]
    [Trait("Extensions", "")]
    public void ShortGuid_Chamado_DeveRetornarGuidValido()
    {
        // Act
        var result = MethodExtensions.OrderGuid();

        // Assert – deve ser parseável de volta para Guid sem lançar exceção
        var parsed = Guid.Parse(result.ToString());
        Assert.Equal(result, parsed);
    }

    [Fact(DisplayName = "OrderGuid deve retornar um Guid no formato padrão de 36 caracteres com hifens")]
    [Trait("Extensions", "")]
    public void ShortGuid_Chamado_DeveRetornarGuidNoFormatoD()
    {
        // Act
        var result = MethodExtensions.OrderGuid();
        var formatted = result.ToString("D"); // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

        // Assert – formato "D" deve ter exatamente 36 caracteres e 4 hifens
        Assert.Equal(36, formatted.Length);
        Assert.Equal(4, formatted.Count(c => c == '-'));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Unicidade
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "OrderGuid deve retornar Guids únicos a cada chamada")]
    [Trait("Extensions", "")]
    public void ShortGuid_ChamadoMaisDeUmaVez_DeveRetornarGuidsUnicos()
    {
        // Act
        var first  = MethodExtensions.OrderGuid();
        var second = MethodExtensions.OrderGuid();

        // Assert
        Assert.NotEqual(first, second);
    }

    [Fact(DisplayName = "OrderGuid deve retornar apenas valores únicos em 1000 chamadas consecutivas")]
    [Trait("Extensions", "")]
    public void ShortGuid_MilChamadas_NaoDeveConterDuplicatas()
    {
        // Arrange
        const int total = 1_000;
        var set = new HashSet<Guid>(total);

        // Act
        for (var i = 0; i < total; i++)
            set.Add(MethodExtensions.OrderGuid());

        // Assert – nenhum duplicado: o conjunto deve ter o mesmo tamanho do total gerado
        Assert.Equal(total, set.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Sequencialidade (indexabilidade)
    // Um COMB SQL ordena pelos últimos 6 bytes que carregam o timestamp.
    // Comparando as representações byte[] na posição correta, o segundo
    // Guid gerado deve ser >= ao primeiro.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "OrderGuid deve ser sequencial: o segundo Guid gerado deve ser maior ou igual ao primeiro")]
    [Trait("Extensions", "")]
    public void ShortGuid_GeradoEmSequencia_SegundoDeveSerMaiorOuIgualAoPrimeiro()
    {
        // Act – pequena pausa garante timestamp distinto entre as chamadas
        var first  = MethodExtensions.OrderGuid();
        Thread.Sleep(1);
        var second = MethodExtensions.OrderGuid();

        // Assert – compara os bytes que carregam o timestamp (posições 10–15 no layout SQL-COMB)
        var firstBytes  = first.ToByteArray();
        var secondBytes = second.ToByteArray();

        // Os 6 bytes de timestamp ficam nas posições 10 a 15 no formato SQL Server
        var timestampComparison = CompareTimestampBytes(firstBytes, secondBytes);
        Assert.True(timestampComparison <= 0,
            $"Esperava que o segundo Guid fosse >= ao primeiro, mas first={first}, second={second}");
    }

    [Fact(DisplayName = "OrderGuid deve gerar sequência ordenável crescente para 100 Guids consecutivos")]
    [Trait("Extensions", "")]
    public void ShortGuid_CemGuidsConsecutivos_DevemFormarSequenciaOrdenada()
    {
        // Arrange
        const int total = 100;
        var guids = new List<Guid>(total);

        for (var i = 0; i < total; i++)
        {
            guids.Add(MethodExtensions.OrderGuid());
            Thread.Sleep(1); // garante ticks distintos
        }

        // Act – ordena uma cópia e compara com a lista original
        var sorted = guids.OrderBy(g => g.ToByteArray(), new TimestampBytesComparer()).ToList();

        // Assert – a ordem gerada deve ser igual à ordem cronológica
        Assert.Equal(guids, sorted);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tipo de retorno
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "OrderGuid deve retornar o tipo System.Guid")]
    [Trait("Extensions", "")]
    public void ShortGuid_Chamado_DeveRetornarTipoGuid()
    {
        // Act
        var result = MethodExtensions.OrderGuid();

        // Assert
        Assert.IsType<Guid>(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Compara apenas os 6 bytes de timestamp (posições 10–15) de dois arrays de bytes de Guid.
    /// Retorna negativo se a &lt; b, zero se a == b, positivo se a &gt; b.
    /// </summary>
    private static int CompareTimestampBytes(byte[] a, byte[] b)
    {
        for (var i = 10; i <= 15; i++)
        {
            var cmp = a[i].CompareTo(b[i]);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

    /// <summary>
    /// Comparer para ordenar arrays de bytes de Guid pelo timestamp SQL-COMB (bytes 10–15).
    /// </summary>
    private sealed class TimestampBytesComparer : IComparer<byte[]>
    {
        public int Compare(byte[]? x, byte[]? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return CompareTimestampBytes(x, y);
        }
    }
}
