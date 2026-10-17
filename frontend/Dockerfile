# Etap 1: Budowanie aplikacji Vue.js
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npx vite build

# Etap 2: Serwowanie przez lekki serwer Nginx
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html

# Generujemy plik default.conf bezpośrednio w Linuksie (czyste kodowanie ASCII/LF bez udziału Windowsa)
RUN printf 'server {\n    listen 80;\n    server_name localhost;\n    root /usr/share/nginx/html;\n    index index.html;\n    location / {\n        try_files $uri $uri/ /index.html;\n    }\n}\n' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]