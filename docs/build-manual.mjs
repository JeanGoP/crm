import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

// El Markdown original conserva cada captura junto a la explicación de su módulo.
execFileSync('powershell.exe', ['-NoProfile', '-File', fileURLToPath(new URL('./generate-manual-html.ps1', import.meta.url))], { stdio: 'inherit' });
