# Exact Commands

## Local TeX Live PDF build

```powershell
docker run --rm -v "C:\Users\V\Desktop\Roman-AI\paper\information_authority:/work" -w /work texlive/texlive:latest pdflatex -interaction=nonstopmode -halt-on-error main.tex
docker run --rm -v "C:\Users\V\Desktop\Roman-AI\paper\information_authority:/work" -w /work texlive/texlive:latest bibtex main
docker run --rm -v "C:\Users\V\Desktop\Roman-AI\paper\information_authority:/work" -w /work texlive/texlive:latest pdflatex -interaction=nonstopmode -halt-on-error main.tex
docker run --rm -v "C:\Users\V\Desktop\Roman-AI\paper\information_authority:/work" -w /work texlive/texlive:latest pdflatex -interaction=nonstopmode -halt-on-error main.tex
```

## Docker reproduction

```powershell
docker build -f paper/repro/Dockerfile -t roman-ai-paper-repro:20260525 .
docker run --rm -v "C:\Users\V\Desktop\Roman-AI\paper\repro\out:/workspace/out" roman-ai-paper-repro:20260525
```

## Local .NET verifier

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Romana\Tools\Test-PaperReadyBundle.ps1 -ProjectPath .\Romana
```
