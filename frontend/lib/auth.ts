import NextAuth from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

export const { handlers, auth, signIn, signOut } = NextAuth({
  trustHost: true,
  secret: process.env.AUTH_SECRET ?? process.env.NEXTAUTH_SECRET,
  providers: [
    Keycloak({
      clientId: process.env.KEYCLOAK_CLIENT_ID!,
      clientSecret: process.env.KEYCLOAK_CLIENT_SECRET ?? "",
      issuer: process.env.KEYCLOAK_ISSUER!,
    }),
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      // Saat pertama login — simpan semua data dari Keycloak
      if (account && profile) {
        token.accessToken  = account.access_token;
        token.idToken      = account.id_token;
        token.refreshToken = account.refresh_token;

        // Roles dari realm_access (Keycloak specific)
        const realmAccess = (profile as any)?.realm_access;
        token.roles = realmAccess?.roles ?? [];
      }

      // Kalau roles belum ada (misalnya token di-decode ulang dari cookie),
      // decode manual dari accessToken JWT
      if (!token.roles || (token.roles as string[]).length === 0) {
        try {
          if (token.accessToken) {
            const payload = JSON.parse(
              Buffer.from((token.accessToken as string).split(".")[1], "base64").toString()
            );
            token.roles = payload?.realm_access?.roles ?? [];
          }
        } catch { /* ignore */ }
      }

      return token;
    },
    async session({ session, token }) {
      session.accessToken = token.accessToken as string;
      session.idToken     = token.idToken     as string;
      session.roles       = (token.roles as string[]) ?? [];
      return session;
    },
  },
  events: {
    async signOut(message) {
      const idToken = (message as any)?.token?.idToken;
      if (idToken && process.env.KEYCLOAK_ISSUER) {
        const logoutUrl =
          `${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/logout` +
          `?id_token_hint=${idToken}` +
          `&post_logout_redirect_uri=${encodeURIComponent(process.env.NEXTAUTH_URL ?? "http://localhost:3000")}`;
        await fetch(logoutUrl).catch(() => {});
      }
    },
  },
  pages: {
    signIn: "/login",
  },
});
