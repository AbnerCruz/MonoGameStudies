"""Pack every file required by the published player into one gzip JSON asset."""
import base64
import gzip
import json
import pathlib
import sys

player_dir = pathlib.Path(sys.argv[1])
framework = player_dir / '_framework'
assets = {}
for path in sorted(framework.rglob('*')):
    if not path.is_file() or path.suffix in ('.gz', '.br', '.pdb'):
        continue
    assets[path.relative_to(framework).as_posix()] = base64.b64encode(path.read_bytes()).decode('ascii')
output = pathlib.Path(sys.argv[2])
output.write_bytes(gzip.compress(json.dumps(assets, separators=(',', ':')).encode(), compresslevel=9))
print(f'{len(assets)} resources, {output.stat().st_size / 1048576:.1f} MiB packed')
