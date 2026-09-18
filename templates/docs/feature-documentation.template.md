<!--
Template: Feature Documentation
For a specific feature/capability, once implemented. Document the why and the non-obvious
behavior, not a restatement of the code -- see skills/documentation/technical-writing.md.
-->

# {{FEATURE_NAME}}

## Summary

{{1-3 sentences: what this feature does and who/what uses it.}}

## Why It Exists

{{The requirement/problem that motivated this feature -- link to the originating issue/
ADR if one exists rather than restating it.}}

## How It Works

{{The behavior from the user's/caller's perspective first -- inputs, outputs, side
effects. Then, if needed, the key implementation points that aren't obvious from reading
the code (a non-obvious algorithm choice, a workaround for a specific constraint).}}

## Configuration

{{Any config keys/feature flags controlling this feature's behavior, and their defaults.}}

## Edge Cases and Known Limitations

{{What this feature deliberately does NOT handle, and why -- saves someone from
"discovering" a known limitation and re-litigating it.}}

## Related Code

{{Pointers to the key files/classes -- entry point, main logic, tests -- for someone who
needs to change this feature later.}}

## Related Documentation

{{Links to the relevant skill(s), ADR(s), or API documentation.}}
