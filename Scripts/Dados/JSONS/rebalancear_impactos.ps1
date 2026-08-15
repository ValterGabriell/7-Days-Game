# Script PowerShell para rebalancear impactos nos arquivos JSON

function Rebalancear-Impacto {
    param(
        [hashtable]$impacto,
        [string]$tipo
    )
    
    $esp = [double]$impacto.VariacaoEsperanca
    $irr = [double]$impacto.VariacaoIrritacao
    $aud = [double]$impacto.AudienciaGanha
    
    switch ($tipo) {
        "ORIGINAL" {
            $novoEsp = [Math]::Max([Math]::Min($esp * 0.6, 3), -3)
            $novoIrr = if ($irr -gt 0) { [Math]::Max([Math]::Min($irr * 0.5, 4), 1) } else { $irr * 0.5 }
            $novoAud = [Math]::Max([Math]::Min($aud * 0.5, 5), 2)
        }
        "OMITIR" {
            $novoEsp = [Math]::Max([Math]::Min($esp * 0.7, 2), -1)
            $novoIrr = [Math]::Max([Math]::Min($irr * 0.5, 1), -2)
            $novoAud = [Math]::Max([Math]::Min($aud * 0.6, 3), 1)
        }
        "MENTIR" {
            $novoEsp = if ($esp -gt 0) { [Math]::Max([Math]::Min($esp * 0.7, 12), 6) } else { $esp * 0.5 }
            $novoIrr = if ($irr -lt 0) { [Math]::Max([Math]::Min($irr * 0.7, -6), -10) } else { $irr * 0.5 }
            $novoAud = [Math]::Max([Math]::Min($aud * 0.6, 8), 5)
        }
        "DISTORCER" {
            $novoEsp = if ($esp -lt 0) { [Math]::Max([Math]::Min($esp * 0.6, -9), -15) } else { $esp * 0.5 }
            $novoIrr = if ($irr -gt 0) { [Math]::Max([Math]::Min($irr * 0.5, 14), 10) } else { $irr * 0.5 }
            $novoAud = [Math]::Max([Math]::Min($aud * 0.45, 12), 8)
        }
    }
    
    return @{
        VariacaoEsperanca = [Math]::Round($novoEsp, 1)
        VariacaoIrritacao = [Math]::Round($novoIrr, 1)
        AudienciaGanha = [Math]::Round($novoAud, 1)
    }
}

function Processar-Arquivo {
    param([string]$caminho)
    
    Write-Host "Processando: $caminho"
    
    $json = Get-Content $caminho -Raw -Encoding UTF8 | ConvertFrom-Json
    $alteracoes = 0
    
    foreach ($noticia in $json) {
        if ($noticia.Variacoes) {
            foreach ($tipo in @("ORIGINAL", "OMITIR", "MENTIR", "DISTORCER")) {
                if ($noticia.Variacoes.$tipo.Impacto) {
                    $impactoOriginal = @{
                        VariacaoEsperanca = $noticia.Variacoes.$tipo.Impacto.VariacaoEsperanca
                        VariacaoIrritacao = $noticia.Variacoes.$tipo.Impacto.VariacaoIrritacao
                        AudienciaGanha = $noticia.Variacoes.$tipo.Impacto.AudienciaGanha
                    }
                    
                    $impactoNovo = Rebalancear-Impacto -impacto $impactoOriginal -tipo $tipo
                    
                    $noticia.Variacoes.$tipo.Impacto.VariacaoEsperanca = $impactoNovo.VariacaoEsperanca
                    $noticia.Variacoes.$tipo.Impacto.VariacaoIrritacao = $impactoNovo.VariacaoIrritacao
                    $noticia.Variacoes.$tipo.Impacto.AudienciaGanha = $impactoNovo.AudienciaGanha
                    $alteracoes++
                }
            }
        }
    }
    
    # Salva com indentação correta
    $json | ConvertTo-Json -Depth 10 | Set-Content $caminho -Encoding UTF8
    Write-Host "OK: $alteracoes impactos rebalanceados"
    
    return $alteracoes
}

# Processa dias 2 a 7
$pastaBase = "c:\Dev\ProjetosPessoais\five-years-3\Scripts\Dados\JSONS"
$totalAlteracoes = 0

for ($dia = 2; $dia -le 7; $dia++) {
    $numeroDia = "{0:D2}" -f $dia
    $caminho = Join-Path $pastaBase "Dia_$numeroDia\Noticias.json"
    if (Test-Path $caminho) {
        $alteracoes = Processar-Arquivo -caminho $caminho
        $totalAlteracoes += $alteracoes
    } else {
        Write-Host "Arquivo nao encontrado: $caminho"
    }
}

Write-Host ""
Write-Host "Total: $totalAlteracoes impactos rebalanceados em 6 dias"

