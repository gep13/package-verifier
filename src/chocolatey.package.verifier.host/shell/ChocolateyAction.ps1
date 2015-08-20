#todo: For upgrade - Install the older version first.

$env:PATH +=';c:\ProgramData\chocolatey\bin'

choco {{action}} {{package}} --version {{version}} -dvy