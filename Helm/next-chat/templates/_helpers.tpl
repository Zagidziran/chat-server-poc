{{- define "stack.name" -}}
{{ .Values.deployment.stackName | default .Release.Name }}
{{- end -}}

{{- define "image.tag" -}}
{{ .Values.docker.image.tag | default .Chart.AppVersion }}
{{- end -}}

{{- define "tests.name" -}}
{{ template "stack.name" . }}-tests
{{- end -}}

{{- define "configure.k8s.objectName" -}}
{{ template "stack.name" . }}-{{ .Values.configure.appName }}
{{- end -}}

{{- define "privateWeb.k8s.objectName" -}}
{{ template "stack.name" . }}-{{ .Values.privateWeb.appName }}
{{- end -}}

{{- define "consoleService.k8s.objectName" -}}
{{ template "stack.name" . }}-{{ .Values.consoleService.appName }}
{{- end -}}

{{- define "logs.elasticSearch.uri" -}}
{{- join "," .Values.logs.elasticSearch.connectionPool }}
{{- end -}}
