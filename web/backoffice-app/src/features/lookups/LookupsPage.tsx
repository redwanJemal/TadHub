import { useTranslation } from "react-i18next";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";

export function LookupsPage() {
  const { t } = useTranslation("lookups");

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{t("title")}</h1>
        <p className="text-muted-foreground">{t("subtitle")}</p>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>{t("list")}</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground">Lookups will appear here</p>
        </CardContent>
      </Card>
    </div>
  );
}
