{{/*
Expand the name of the chart.
*/}}
{{- define "careops.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Full image reference for a given service.
Usage: {{ include "careops.image" (dict "registry" .Values.global.imageRegistry "name" "patient-service" "tag" .Values.global.imageTag) }}
*/}}
{{- define "careops.image" -}}
{{ .registry }}/{{ .name }}:{{ .tag }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "careops.labels" -}}
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version }}
{{- end }}
