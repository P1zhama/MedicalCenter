FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
COPY packages/shared/package.json ./packages/shared/
COPY ClientWebApp/package.json ./ClientWebApp/
COPY InternalClinicPortal/package.json ./InternalClinicPortal/

RUN npm ci

COPY . .

RUN npm run build --workspace internalclinicportal

FROM nginx:1.27-alpine AS runtime

COPY InternalClinicPortal/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/InternalClinicPortal/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s \
    CMD wget --quiet --tries=1 --spider http://127.0.0.1/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
